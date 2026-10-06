namespace CRISP.Application.DTOs;

public sealed class FrameworkDto
{
    public WorkflowSummaryDto Workflow { get; set; } = new();
    public List<ParameterDto> Parameters { get; set; } = [];

    public int Id { get; set; }
    public int EntityId { get; set; }
    public string WorkflowCode { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string IndustryName { get; set; } = string.Empty;
    public string FrameworkName { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public int WorkflowVersionNo { get; set; }
    public DateTime FrameworkDate { get; set; }
    public bool ConsolidatedView { get; set; }
    public List<FrameworkStepDto> Steps { get; set; } = [];
    public List<BusinessParameterDetailsDto> IndustrySegments { get; set; } = [];
    public List<FrameworkParameterDto> ParameterValues { get; set; } = [];
}

public sealed class FrameworkEvaluationRequestDto
{
    public string WorkflowCode { get; set; } = string.Empty;
    public Dictionary<string, int> Scores { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class WorkflowSummaryDto
{
    public int Id { get; set; }
    public string WorkflowCode { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
