namespace CRISP.Domain.Entities;

public class IndustryDetails
{
    public int Id { get; set; }
    public int WorkflowMasterId { get; set; }
    public string IndustryName { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public WorkflowMaster WorkflowMaster { get; set; } = null!;
    public ICollection<BusinessParameterDetails> FrameworkSegments { get; set; } = new List<BusinessParameterDetails>();
}
