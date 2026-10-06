using System.Text.Json;
using CRISP.Application.DTOs;
using CRISP.Application.Interfaces;

namespace CRISP.Application.Services;

public sealed class RuleEvaluationService(
    IFrameworkRepository repository,
    IRuleEngineExecutor ruleEngine) : IRuleEvaluationService
{
    public async Task<EvaluateResponse> EvaluateAsync(
        EvaluateRequest request,
        CancellationToken ct = default)
    {
        var version = await repository.GetVersionAsync(
            request.WorkflowCode,
            request.VersionNo,
            ct);

        if (version is null)
        {
            return new EvaluateResponse(
                false,
                0,
                request.VersionNo,
                string.Empty,
                "Workflow JSON version not found.");
        }

        var parameters = await repository.GetParametersAsync(
            request.WorkflowCode,
            ct);

        var scores =
            new Dictionary<string, decimal>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var parameter in parameters)
        {
            /*
             * Value-driven parameter
             *
             * Example:
             * MarketPositionScore = value 52
             *
             * The score is determined from the
             * ScoreDefinition stored in the database.
             */
            if (parameter.IsValueDriven &&
                request.Values.TryGetValue(
                    parameter.ParameterCode,
                    out var value) &&
                value.HasValue)
            {
                var matching = parameter.Scores
                    .Where(x =>
                        (
                            !x.MinValue.HasValue ||
                            (
                                x.MinInclusive
                                    ? value.Value >= x.MinValue.Value
                                    : value.Value > x.MinValue.Value
                            )
                        )
                        &&
                        (
                            !x.MaxValue.HasValue ||
                            (
                                x.MaxInclusive
                                    ? value.Value <= x.MaxValue.Value
                                    : value.Value < x.MaxValue.Value
                            )
                        )
                    )
                    .OrderBy(x => x.Score)
                    .FirstOrDefault();

                if (matching is null)
                {
                    return new EvaluateResponse(
                        false,
                        0,
                        request.VersionNo,
                        string.Empty,
                        $"No score definition matches value {value} for {parameter.ParameterName}.");
                }

                scores[parameter.ParameterCode] =
                    matching.Score;

                continue;
            }

            /*
             * Manual score
             *
             * These values come directly from the UI.
             * Nothing is hardcoded here.
             */
            if (request.ManualScores.TryGetValue(
                    parameter.ParameterCode,
                    out var manualScore))
            {
                scores[parameter.ParameterCode] =
                    manualScore;

                continue;
            }

            /*
             * If no value/score was supplied,
             * send zero to the Rules Engine.
             */
            scores[parameter.ParameterCode] = 0;
        }

        /*
         * IMPORTANT
         *
         * The Rules Engine workflow name must match
         * the "WorkflowName" inside the JSON.
         *
         * JSON:
         *
         * "WorkflowName": "CorporateBusiness"
         *
         * Therefore use WorkflowCode rather than
         * WorkflowMaster.WorkflowName
         * ("Corporate Business Framework").
         */
        var workflowName = request.WorkflowCode;

        var result = await ruleEngine.ExecuteAsync(
            version.JsonContent,
            workflowName,
            scores,
            ct);

        return new EvaluateResponse(
            result.Success,
            result.Score,
            request.VersionNo,
            result.Formula,
            result.Message);
    }
}