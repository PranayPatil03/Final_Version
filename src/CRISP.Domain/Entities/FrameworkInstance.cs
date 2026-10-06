namespace CRISP.Domain.Entities;

public class FrameworkInstance
{
    public int Id { get; set; }
    public int WorkflowMasterId { get; set; }
    public int EntityId { get; set; }
    public string EntityName { get; set; } = "";
    public string IndustryName { get; set; } = "";
    public string FrameworkName { get; set; } = "";
    public string Status { get; set; } = "Draft";
    public int WorkflowVersionNo { get; set; }
    public DateTime FrameworkDate { get; set; }
    public bool ConsolidatedView { get; set; }
    public WorkflowMaster WorkflowMaster { get; set; } = null!;
    public CustomerMaster Entity { get; set; } = null!;
    public ICollection<BusinessParameterDetails> IndustrySegments { get; set; } = new List<BusinessParameterDetails>();
    public ICollection<FrameworkParameterValue> ParameterValues { get; set; } = new List<FrameworkParameterValue>();
}
