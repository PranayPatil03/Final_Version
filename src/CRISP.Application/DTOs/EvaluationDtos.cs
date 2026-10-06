namespace CRISP.Application.DTOs;

public sealed class EvaluateRequest
{
    public EvaluateRequest() { }

    public EvaluateRequest(
        string workflowCode,
        int versionNo,
        Dictionary<string, decimal?> values,
        Dictionary<string, int> manualScores)
    {
        WorkflowCode = workflowCode;
        VersionNo = versionNo;
        Values = values;
        ManualScores = manualScores;
    }

    public string WorkflowCode { get; set; } = string.Empty;
    public int VersionNo { get; set; }
    public Dictionary<string, decimal?> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> ManualScores { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class EvaluateResponse
{
    public EvaluateResponse() { }

    public EvaluateResponse(
        bool success,
        decimal weightedAverageScore,
        int versionNo,
        string formula,
        string message)
    {
        Success = success;
        WeightedAverageScore = weightedAverageScore;
        VersionNo = versionNo;
        Formula = formula;
        Message = message;
    }

    public bool Success { get; set; }
    public decimal WeightedAverageScore { get; set; }
    public int VersionNo { get; set; }
    public string Formula { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class SaveVersionRequest
{
    public SaveVersionRequest() { }

    public SaveVersionRequest(
        string workflowCode,
        int versionNo,
        string versionName,
        string status,
        string jsonContent,
        bool isPublished)
    {
        WorkflowCode = workflowCode;
        VersionNo = versionNo;
        VersionName = versionName;
        Status = status;
        JsonContent = jsonContent;
        IsPublished = isPublished;
    }

    public string WorkflowCode { get; set; } = string.Empty;
    public int VersionNo { get; set; }
    public string VersionName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string JsonContent { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
}
