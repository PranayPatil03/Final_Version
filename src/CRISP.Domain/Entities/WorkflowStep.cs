namespace CRISP.Domain.Entities;

public class WorkflowStep
{
    public int Id { get; set; }
    public int WorkflowMasterId { get; set; }
    public string StepName { get; set; } = "";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public WorkflowMaster WorkflowMaster { get; set; } = null!;
}
