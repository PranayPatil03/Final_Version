namespace CRISP.Application.Interfaces;

public interface IRuleEngineExecutor
{
    Task<RuleEngineExecutionResult> ExecuteAsync(
        string workflowJson,
        string workflowName,
        IReadOnlyDictionary<string, decimal> scores,
        CancellationToken ct = default);
}

public sealed record RuleEngineExecutionResult(
    bool Success,
    decimal Score,
    string Formula,
    string Message);
