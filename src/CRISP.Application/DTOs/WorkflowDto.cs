namespace CRISP.Application.DTOs;

public sealed class WorkflowDto
{
    public WorkflowDto() { }
    public WorkflowDto(int id, string workflowCode, string workflowName, bool isActive)
    { Id = id; WorkflowCode = workflowCode; WorkflowName = workflowName; IsActive = isActive; }
    public int Id { get; set; }
    public string WorkflowCode { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
