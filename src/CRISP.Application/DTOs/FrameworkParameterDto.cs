namespace CRISP.Application.DTOs;

public sealed class FrameworkParameterDto
{
    public FrameworkParameterDto() { }
    public FrameworkParameterDto(int parameterId, string parameterName, decimal weight, decimal? value, int score)
    { ParameterId = parameterId; ParameterName = parameterName; Weight = weight; Value = value; Score = score; }
    public int ParameterId { get; set; }
    public string ParameterName { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal? Value { get; set; }
    public int Score { get; set; }
}
