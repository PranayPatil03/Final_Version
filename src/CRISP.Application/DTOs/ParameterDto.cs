namespace CRISP.Application.DTOs;

public sealed class ParameterDto
{
    public ParameterDto() { }
    public ParameterDto(int id, string parameterCode, string parameterName, string whyItMatters, string howDoWeMeasure, decimal weight, int displayOrder, bool isValueDriven, IReadOnlyList<ScoreDto> scores)
    {
        Id = id; ParameterCode = parameterCode; ParameterName = parameterName; WhyItMatters = whyItMatters;
        HowDoWeMeasure = howDoWeMeasure; Weight = weight; DisplayOrder = displayOrder; IsValueDriven = isValueDriven;
        Scores = scores;
    }

    public int Id { get; set; }
    public string ParameterCode { get; set; } = string.Empty;
    public string ParameterName { get; set; } = string.Empty;
    public string WhyItMatters { get; set; } = string.Empty;
    public string HowDoWeMeasure { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsValueDriven { get; set; }
    public IReadOnlyList<ScoreDto> Scores { get; set; } = [];
}
