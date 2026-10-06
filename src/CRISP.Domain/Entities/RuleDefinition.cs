namespace CRISP.Domain.Entities;

public class RuleDefinition
{
    public int Id { get; set; }
    public int WorkflowMasterId { get; set; }
    public string RuleName { get; set; } = "";
    public string SuccessEvent { get; set; } = "Calculated";
    public string Expression { get; set; } = "true";
    public string RuleExpressionType { get; set; } = "LambdaExpression";
    public string OutputExpression { get; set; } = "";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public WorkflowMaster WorkflowMaster { get; set; } = null!;
}
