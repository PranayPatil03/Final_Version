namespace CRISP.Domain.Entities;

public class ScoreDefinition
{
    public int Id { get; set; }
    public int ParameterDefinitionId { get; set; }
    public int Score { get; set; }
    public string Definition { get; set; } = "";
    public decimal? ThresholdValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public bool MinInclusive { get; set; } = true;
    public bool MaxInclusive { get; set; } = true;
    public string? Unit { get; set; }
    public int RowNo { get; set; }
    public int ColNo { get; set; }
    public ParameterDefinition ParameterDefinition { get; set; } = null!;
}
