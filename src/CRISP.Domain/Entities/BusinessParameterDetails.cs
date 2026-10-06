namespace CRISP.Domain.Entities;

public class BusinessParameterDetails
{
    public int Id { get; set; }
    public int FrameworkInstanceId { get; set; }
    public int IndustrySegmentId { get; set; }
    public decimal Weight { get; set; }
    public string Status { get; set; } = "Saved";
    public FrameworkInstance FrameworkInstance { get; set; } = null!;
    public IndustryDetails IndustrySegment { get; set; } = null!;
}
