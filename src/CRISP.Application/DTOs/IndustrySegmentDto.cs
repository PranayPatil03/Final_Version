namespace CRISP.Application.DTOs;

public sealed class IndustrySegmentDto
{
    public IndustrySegmentDto() { }
    public IndustrySegmentDto(int id, string industryName) { Id = id; IndustryName = industryName; }
    public int Id { get; set; }
    public string IndustryName { get; set; } = string.Empty;
}
