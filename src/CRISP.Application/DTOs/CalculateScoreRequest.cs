namespace CRISP.Application.DTOs;

public sealed class CalculateScoreRequest
{
    public CalculateScoreRequest() { }
    public CalculateScoreRequest(int parameterId, decimal? value) { ParameterId = parameterId; Value = value; }
    public int ParameterId { get; set; }
    public decimal? Value { get; set; }
}
