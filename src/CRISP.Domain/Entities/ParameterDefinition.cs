namespace CRISP.Domain.Entities;

public class ParameterDefinition
{
    public int Id { get; set; }
    public int WorkflowMasterId { get; set; }
    public string ParameterCode { get; set; } = "";
    public string ParameterName { get; set; } = "";
    public string WhyItMatters { get; set; } = "";
    public string HowDoWeMeasure { get; set; } = "";
    public decimal Weight { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsValueDriven { get; set; }
    public WorkflowMaster WorkflowMaster { get; set; } = null!;
    public ICollection<ScoreDefinition> Scores { get; set; } = new List<ScoreDefinition>();
}
