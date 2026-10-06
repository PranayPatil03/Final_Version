using CRISP.Application.DTOs;
using CRISP.Domain.Entities;

namespace CRISP.Application.Interfaces;

public interface IFrameworkRepository
{
    Task<IReadOnlyList<WorkflowDto>> GetWorkflowsAsync(CancellationToken ct = default);
    Task<WorkflowContextDto?> GetWorkflowContextAsync(string workflowCode, CancellationToken ct = default);
    Task<IReadOnlyList<EntityDto>> SearchEntitiesAsync(string? search, CancellationToken ct = default);
    Task<IReadOnlyList<FrameworkLookupDto>> GetFrameworkOptionsAsync(int entityId, CancellationToken ct = default);
    Task<IReadOnlyList<IndustrySegmentDto>> GetIndustrySegmentsAsync(string workflowCode, CancellationToken ct = default);
    Task<IReadOnlyList<FrameworkStepDto>> GetStepsAsync(string workflowCode, CancellationToken ct = default);
    Task<IReadOnlyList<FrameworkListDto>> GetFrameworksAsync(CancellationToken ct = default);
    Task<WorkflowMaster?> GetWorkflowAsync(string code, CancellationToken ct = default);
    Task<ParameterDto?> GetParameterAsync(int parameterId, CancellationToken ct = default);
    Task<IReadOnlyList<ParameterDto>> GetParametersAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<VersionDto>> GetVersionsAsync(string code, CancellationToken ct = default);
    Task<WorkflowJsonVersion?> GetVersionAsync(string code, int versionNo, CancellationToken ct = default);
    Task<FrameworkDto?> GetFrameworkAsync(int id, CancellationToken ct = default);
    Task<FrameworkDto> SaveFrameworkAsync(FrameworkDto framework, CancellationToken ct = default);
    Task<VersionDto> SaveVersionAsync(SaveVersionRequest request, CancellationToken ct = default);
    Task<WorkflowDefinitionDto?> GetWorkflowDefinitionAsync(string workflowCode, CancellationToken ct = default);
    Task<VersionDto> SaveWorkflowDefinitionAsync(SaveWorkflowDefinitionRequest request, string jsonContent, CancellationToken ct = default);
}
