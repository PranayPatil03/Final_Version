namespace CRISP.Application.DTOs;

public sealed class WorkflowContextDto
{
    public WorkflowDto Workflow { get; set; } = new();
    public List<FrameworkStepDto> Steps { get; set; } = [];
}
