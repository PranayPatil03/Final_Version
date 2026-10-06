using CRISP.Application.DTOs;
using CRISP.Application.Interfaces;
using CRISP.Domain.Entities;

namespace CRISP.Application.Services;

public sealed class FrameworkService(IFrameworkRepository repository) : IFrameworkService
{
    public Task<IReadOnlyList<WorkflowDto>> GetWorkflowsAsync(CancellationToken ct = default) => repository.GetWorkflowsAsync(ct);
    public Task<WorkflowContextDto?> GetWorkflowContextAsync(string workflowCode, CancellationToken ct = default) => repository.GetWorkflowContextAsync(workflowCode, ct);
    public Task<IReadOnlyList<EntityDto>> SearchEntitiesAsync(string? search, CancellationToken ct = default) => repository.SearchEntitiesAsync(search, ct);
    public Task<IReadOnlyList<FrameworkLookupDto>> GetFrameworkOptionsAsync(int entityId, CancellationToken ct = default) => repository.GetFrameworkOptionsAsync(entityId, ct);
    public Task<IReadOnlyList<IndustrySegmentDto>> GetIndustrySegmentsAsync(string workflowCode, CancellationToken ct = default) => repository.GetIndustrySegmentsAsync(workflowCode, ct);
    public Task<IReadOnlyList<FrameworkStepDto>> GetStepsAsync(string workflowCode, CancellationToken ct = default) => repository.GetStepsAsync(workflowCode, ct);
    public Task<IReadOnlyList<FrameworkListDto>> GetFrameworksAsync(CancellationToken ct = default) => repository.GetFrameworksAsync(ct);
    public Task<FrameworkDto?> GetFrameworkAsync(int id, CancellationToken ct = default) => repository.GetFrameworkAsync(id, ct);
    public Task<IReadOnlyList<ParameterDto>> GetParametersAsync(string workflowCode, CancellationToken ct = default) => repository.GetParametersAsync(workflowCode, ct);
    public Task<IReadOnlyList<VersionDto>> GetVersionsAsync(string workflowCode, CancellationToken ct = default) => repository.GetVersionsAsync(workflowCode, ct);
    public Task<WorkflowJsonVersion?> GetVersionAsync(string workflowCode, int versionNo, CancellationToken ct = default) => repository.GetVersionAsync(workflowCode, versionNo, ct);

    public async Task<CalculateScoreResponse?> CalculateScoreAsync(int parameterId, decimal? value, CancellationToken ct = default)
    {
        var parameter = await repository.GetParameterAsync(parameterId, ct);
        if (parameter is null) return null;
        if (!parameter.IsValueDriven || value is null)
        {
            var first = parameter.Scores.OrderBy(x => x.Score).FirstOrDefault();
            return first is null ? null : new CalculateScoreResponse(parameter.Id, first.Score, first.Definition);
        }
        var match = parameter.Scores
            .Where(x => (!x.MinValue.HasValue || (x.MinInclusive ? value.Value >= x.MinValue.Value : value.Value > x.MinValue.Value))
                     && (!x.MaxValue.HasValue || (x.MaxInclusive ? value.Value <= x.MaxValue.Value : value.Value < x.MaxValue.Value)))
            .OrderBy(x => x.Score).FirstOrDefault();
        return match is null ? null : new CalculateScoreResponse(parameter.Id, match.Score, match.Definition);
    }

    public async Task<FrameworkDto> SaveFrameworkAsync(FrameworkDto framework, CancellationToken ct = default)
    {
        var parameters = await repository.GetParametersAsync(framework.WorkflowCode, ct);
        var parameterMap = parameters.ToDictionary(x => x.Id);
        var normalized = framework.ParameterValues.Select(item =>
        {
            if (!parameterMap.TryGetValue(item.ParameterId, out var definition) || !definition.IsValueDriven || !item.Value.HasValue)
                return item;
            var match = definition.Scores
                .Where(x => (!x.MinValue.HasValue || (x.MinInclusive ? item.Value.Value >= x.MinValue.Value : item.Value.Value > x.MinValue.Value))
                         && (!x.MaxValue.HasValue || (x.MaxInclusive ? item.Value.Value <= x.MaxValue.Value : item.Value.Value < x.MaxValue.Value)))
                .OrderBy(x => x.Score).FirstOrDefault();
            if (match is null)
            {
                return new FrameworkParameterDto(item.ParameterId, item.ParameterName, item.Weight, item.Value, 0);
            }

            return new FrameworkParameterDto(item.ParameterId, item.ParameterName, item.Weight, item.Value, match.Score);
        }).ToList();
        framework.ParameterValues = normalized;
        return await repository.SaveFrameworkAsync(framework, ct);
    }

    public Task<VersionDto> SaveVersionAsync(SaveVersionRequest request, CancellationToken ct = default) => repository.SaveVersionAsync(request, ct);

    public Task<WorkflowDefinitionDto?> GetWorkflowDefinitionAsync(string workflowCode, CancellationToken ct = default) =>
        repository.GetWorkflowDefinitionAsync(workflowCode, ct);

    public async Task<VersionDto> SaveWorkflowDefinitionAsync(SaveWorkflowDefinitionRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.WorkflowCode)) throw new InvalidOperationException("Workflow code is required.");
        if (request.Definition.Parameters.Count == 0) throw new InvalidOperationException("Add at least one parameter before saving the workflow definition.");
        var weight = request.Definition.Parameters.Sum(x => x.Weight);
        if (weight <= 0) throw new InvalidOperationException("Parameter weights must be greater than zero.");

        var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = null };
        var json = System.Text.Json.JsonSerializer.Serialize(new[] { request.Definition }, jsonOptions);
        request.VersionName = string.IsNullOrWhiteSpace(request.VersionName)
            ? $"{DateTime.Today:dd-MMM-yyyy} - Draft"
            : request.VersionName.Trim();
        return await repository.SaveWorkflowDefinitionAsync(request, json, ct);
    }
}