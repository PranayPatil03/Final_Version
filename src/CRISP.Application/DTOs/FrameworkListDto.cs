namespace CRISP.Application.DTOs;

public sealed class FrameworkListDto
{
    public FrameworkListDto() { }
    public FrameworkListDto(int id, string entityName, string industryName, string frameworkName, string status, int workflowVersionNo, DateTime frameworkDate)
    { Id = id; EntityName = entityName; IndustryName = industryName; FrameworkName = frameworkName; Status = status; WorkflowVersionNo = workflowVersionNo; FrameworkDate = frameworkDate; }
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string IndustryName { get; set; } = string.Empty;
    public string FrameworkName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int WorkflowVersionNo { get; set; }
    public DateTime FrameworkDate { get; set; }
}
