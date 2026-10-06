namespace CRISP.Web.Models;
public record ApiVersionDetail(int Id, int VersionNo, string VersionName, string Status, bool IsPublished, DateTime CreatedOn, string CreatedBy, string JsonContent);
