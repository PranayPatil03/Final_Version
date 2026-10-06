namespace CRISP.Application.DTOs;

public sealed class VersionDto
{
    public VersionDto() { }
    public VersionDto(int id, int versionNo, string versionName, string status, bool isPublished, DateTime createdOn, string createdBy)
    { Id = id; VersionNo = versionNo; VersionName = versionName; Status = status; IsPublished = isPublished; CreatedOn = createdOn; CreatedBy = createdBy; }
    public int Id { get; set; }
    public int VersionNo { get; set; }
    public string VersionName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
