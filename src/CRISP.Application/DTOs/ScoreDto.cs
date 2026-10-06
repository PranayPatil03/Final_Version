namespace CRISP.Application.DTOs;

public sealed class ScoreDto
{
    public ScoreDto() { }
    public ScoreDto(int score, string definition, decimal? thresholdValue, string? unit, decimal? minValue, decimal? maxValue, bool minInclusive, bool maxInclusive)
    {
        Score = score; Definition = definition; ThresholdValue = thresholdValue; Unit = unit;
        MinValue = minValue; MaxValue = maxValue; MinInclusive = minInclusive; MaxInclusive = maxInclusive;
    }

    public int Score { get; set; }
    public string Definition { get; set; } = string.Empty;
    public decimal? ThresholdValue { get; set; }
    public string? Unit { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public bool MinInclusive { get; set; }
    public bool MaxInclusive { get; set; }
}
