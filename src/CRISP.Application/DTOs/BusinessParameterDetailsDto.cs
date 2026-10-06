namespace CRISP.Application.DTOs;

public sealed class BusinessParameterDetailsDto
{
    public BusinessParameterDetailsDto() { }
    public BusinessParameterDetailsDto(int id, int industrySegmentId, string industryName, decimal weight, string status)
    { Id = id; IndustrySegmentId = industrySegmentId; IndustryName = industryName; Weight = weight; Status = status; }
    public int Id { get; set; }
    public int IndustrySegmentId { get; set; }
    public string IndustryName { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string Status { get; set; } = string.Empty;
}
