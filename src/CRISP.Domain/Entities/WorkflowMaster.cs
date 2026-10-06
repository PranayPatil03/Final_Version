namespace CRISP.Domain.Entities;

public class WorkflowMaster
{
    public int Id { get; set; }
    public string WorkflowCode { get; set; } = "";
    public string WorkflowName { get; set; } = "";
    public bool IsActive { get; set; }
    public ICollection<WorkflowJsonVersion> Versions { get; set; } = new List<WorkflowJsonVersion>();
    public ICollection<ParameterDefinition> Parameters { get; set; } = new List<ParameterDefinition>();
    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public ICollection<RuleDefinition> Rules { get; set; } = new List<RuleDefinition>();
}
