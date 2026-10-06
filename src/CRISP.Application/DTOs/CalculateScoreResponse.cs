namespace CRISP.Application.DTOs;

public sealed class CalculateScoreResponse
{
    public CalculateScoreResponse() { }
    public CalculateScoreResponse(int parameterId, int score, string definition) { ParameterId = parameterId; Score = score; Definition = definition; }
    public int ParameterId { get; set; }
    public int Score { get; set; }
    public string Definition { get; set; } = string.Empty;
}
