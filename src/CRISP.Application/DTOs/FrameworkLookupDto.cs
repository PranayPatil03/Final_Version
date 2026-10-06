namespace CRISP.Application.DTOs;

public sealed class FrameworkLookupDto
{
    public FrameworkLookupDto() { }
    public FrameworkLookupDto(int id, string displayName, string status, DateTime frameworkDate, int workflowVersionNo, string industryName)
    { Id = id; DisplayName = displayName; Status = status; FrameworkDate = frameworkDate; WorkflowVersionNo = workflowVersionNo; IndustryName = industryName; }
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime FrameworkDate { get; set; }
    public int WorkflowVersionNo { get; set; }
    public string IndustryName { get; set; } = string.Empty;
}
