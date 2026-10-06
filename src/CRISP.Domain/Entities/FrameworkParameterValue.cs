namespace CRISP.Domain.Entities;

public class FrameworkParameterValue
{
    public int Id { get; set; }
    public int FrameworkInstanceId { get; set; }
    public int ParameterDefinitionId { get; set; }
    public decimal? Value { get; set; }
    public int Score { get; set; }
    public FrameworkInstance FrameworkInstance { get; set; } = null!;
    public ParameterDefinition ParameterDefinition { get; set; } = null!;
}
