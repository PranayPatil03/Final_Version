using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using CRISP.Application.Interfaces;
using RulesEngine.Models;

namespace CRISP.Infrastructure.Rules;

public sealed class RulesEngineExecutor : IRuleEngineExecutor
{
    public async Task<RuleEngineExecutionResult> ExecuteAsync(
        string workflowJson,
        string workflowName,
        IReadOnlyDictionary<string, decimal> scores,
        CancellationToken ct = default)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(workflowJson))
            {
                return new RuleEngineExecutionResult(
                    false,
                    0,
                    string.Empty,
                    "Workflow JSON is empty.");
            }

            var root = JsonNode.Parse(workflowJson)?.AsArray();

            if (root is null || root.Count == 0)
            {
                return new RuleEngineExecutionResult(
                    false,
                    0,
                    string.Empty,
                    "Workflow JSON does not contain a workflow.");
            }

            /*
             * The database JSON is stored as:
             *
             * [
             *   {
             *      "WorkflowName": "...",
             *      "Rules": [...]
             *   }
             * ]
             *
             * Microsoft RulesEngine expects each configuration
             * string to represent ONE Workflow object.
             *
             * Therefore we take the first workflow object and
             * create a completely independent clone.
             */
            var workflow = root[0]?.DeepClone()?.AsObject();

            if (workflow is null)
            {
                return new RuleEngineExecutionResult(
                    false,
                    0,
                    string.Empty,
                    "Invalid workflow JSON.");
            }

            /*
             * Read the rules from the cloned workflow.
             */
            var friendlyRules =
                workflow["Rules"]?.AsArray()
                ?? new JsonArray();

            /*
             * Create the RulesEngine-compatible rules.
             */
            var engineRules = new JsonArray();

            foreach (var item in friendlyRules)
            {
                if (item is null)
                {
                    continue;
                }

                var sourceRule = item.AsObject();

                var ruleName =
                    sourceRule["RuleName"]?.GetValue<string>()
                    ?? "Calculate";

                var successEvent =
                    sourceRule["SuccessEvent"]?.GetValue<string>()
                    ?? "Calculated";

                var expression =
                    sourceRule["Expression"]?.GetValue<string>()
                    ?? "true";

                var expressionType =
                    sourceRule["RuleExpressionType"]?.GetValue<string>()
                    ?? "LambdaExpression";

                var outputExpression =
                    GetOutputExpression(sourceRule);

                var engineRule = new JsonObject
                {
                    ["RuleName"] = ruleName,

                    ["SuccessEvent"] = successEvent,

                    ["Expression"] = expression,

                    ["RuleExpressionType"] = expressionType,

                    ["Actions"] = new JsonObject
                    {
                        ["OnSuccess"] = new JsonObject
                        {
                            ["Name"] = "OutputExpression",

                            ["Context"] = new JsonObject
                            {
                                ["Expression"] = outputExpression
                            }
                        }
                    }
                };

                engineRules.Add(engineRule);
            }

            /*
             * Replace Rules on the CLONED workflow.
             *
             * We are not touching the original JSON node.
             */
            workflow["Rules"] = engineRules;

            /*
             * IMPORTANT:
             *
             * Do NOT create a JsonArray here.
             *
             * RulesEngine expects ONE Workflow JSON object.
             */
            var engineJson = workflow.ToJsonString();

            /*
             * Create Microsoft RulesEngine.
             */
            var engine = new RulesEngine.RulesEngine(
                new[]
                {
                    engineJson
                });

            /*
             * Prepare input for RulesEngine.
             *
             * Example:
             *
             * MarketPosition = 52
             * Scale = 52
             */
            var input = scores.ToDictionary(
                x => x.Key,
                x => (object)x.Value,
                StringComparer.OrdinalIgnoreCase);

            /*
             * Execute the selected workflow.
             */
            var results =
                await engine.ExecuteAllRulesAsync(
                    workflowName,
                    new RuleParameter(
                        "input1",
                        input));

            /*
             * Find CalculateWeightedAverage rule.
             *
             * IMPORTANT:
             *
             * RuleResultTree.Rule.RuleName
             *
             * not:
             *
             * RuleResultTree.RuleName
             */
            var calculationResult =
                results.FirstOrDefault(x =>
                    string.Equals(
                        x.Rule?.RuleName,
                        "CalculateWeightedAverage",
                        StringComparison.OrdinalIgnoreCase));

            /*
             * Fallback:
             *
             * If the JSON uses another calculation rule name,
             * find a successful rule which produced an output.
             */
            calculationResult ??=
                results.FirstOrDefault(x =>
                    x.IsSuccess &&
                    x.ActionResult?.Output is not null);

            if (calculationResult is null)
            {
                return new RuleEngineExecutionResult(
                    false,
                    0,
                    string.Empty,
                    "CalculateWeightedAverage rule did not return a result.");
            }

            if (!calculationResult.IsSuccess)
            {
                return new RuleEngineExecutionResult(
                    false,
                    0,
                    string.Empty,
                    calculationResult.ExceptionMessage
                    ?? "Rule evaluation failed.");
            }

            /*
             * OutputExpression result.
             */
            var outputValue =
                calculationResult.ActionResult?.Output;

            if (outputValue is null)
            {
                return new RuleEngineExecutionResult(
                    false,
                    0,
                    string.Empty,
                    "The workflow did not return a weighted average score.");
            }

            /*
             * Convert RulesEngine output to decimal.
             */
            if (!TryConvertToDecimal(
                    outputValue,
                    out var weightedAverage))
            {
                return new RuleEngineExecutionResult(
                    false,
                    0,
                    string.Empty,
                    $"The weighted average result '{outputValue}' is not a valid number.");
            }

            /*
             * Get calculation formula from original
             * friendly rule definition.
             */
            var formulaRule =
                friendlyRules.FirstOrDefault(item =>
                    string.Equals(
                        item?["RuleName"]?.GetValue<string>(),
                        "CalculateWeightedAverage",
                        StringComparison.OrdinalIgnoreCase));

            var formula =
                formulaRule is null
                    ? string.Empty
                    : GetOutputExpression(
                        formulaRule.AsObject());

            return new RuleEngineExecutionResult(
                true,
                weightedAverage,
                formula,
                "Framework calculated successfully.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new RuleEngineExecutionResult(
                false,
                0,
                string.Empty,
                ex.Message);
        }
    }

    private static string GetOutputExpression(
        JsonObject rule)
    {
        /*
         * Preferred format:
         *
         * "OutputExpression": "..."
         */
        var directExpression =
            rule["OutputExpression"]?.GetValue<string>();

        if (!string.IsNullOrWhiteSpace(directExpression))
        {
            return directExpression;
        }

        /*
         * Also support:
         *
         * Actions
         *   -> OnSuccess
         *      -> Context
         *         -> Expression
         */
        var actions =
            rule["Actions"]?.AsObject();

        var onSuccess =
            actions?["OnSuccess"]?.AsObject();

        var context =
            onSuccess?["Context"]?.AsObject();

        return context?["Expression"]?.GetValue<string>()
               ?? "0";
    }

    private static bool TryConvertToDecimal(
        object value,
        out decimal result)
    {
        switch (value)
        {
            case decimal decimalValue:

                result = decimalValue;
                return true;

            case double doubleValue:

                result = Convert.ToDecimal(
                    doubleValue,
                    CultureInfo.InvariantCulture);

                return true;

            case float floatValue:

                result = Convert.ToDecimal(
                    floatValue,
                    CultureInfo.InvariantCulture);

                return true;

            case int intValue:

                result = intValue;
                return true;

            case long longValue:

                result = longValue;
                return true;

            case JsonElement element:

                if (element.ValueKind ==
                    JsonValueKind.Number &&
                    element.TryGetDecimal(out var number))
                {
                    result = number;
                    return true;
                }

                if (element.ValueKind ==
                    JsonValueKind.String &&
                    decimal.TryParse(
                        element.GetString(),
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var parsedElement))
                {
                    result = parsedElement;
                    return true;
                }

                break;

            case string text when
                decimal.TryParse(
                    text,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var parsedText):

                result = parsedText;
                return true;
        }

        if (decimal.TryParse(
                value.ToString(),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var parsed))
        {
            result = parsed;
            return true;
        }

        result = 0;
        return false;
    }
}