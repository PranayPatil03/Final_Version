namespace CRISP.Domain.Entities;

public class WorkflowJsonVersion
{
    public int Id { get; set; }
    public int WorkflowMasterId { get; set; }
    public int VersionNo { get; set; }
    public string VersionName { get; set; } = "";
    public string Status { get; set; } = "Draft";
    public string JsonContent { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = "System";
    public bool IsPublished { get; set; }
    public WorkflowMaster WorkflowMaster { get; set; } = null!;
}
