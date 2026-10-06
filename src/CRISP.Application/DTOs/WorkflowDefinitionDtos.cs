using System.Text.Json.Serialization;

namespace CRISP.Application.DTOs;

public sealed class WorkflowDefinitionDto
{
    [JsonPropertyName("WorkflowName")] public string WorkflowName { get; set; } = string.Empty;
    [JsonPropertyName("GlobalParams")] public List<WorkflowGlobalParameterDto> GlobalParams { get; set; } = [];
    [JsonPropertyName("Steps")] public List<WorkflowJsonStepDto> Steps { get; set; } = [];
    [JsonPropertyName("Parameters")] public List<WorkflowJsonParameterDto> Parameters { get; set; } = [];
    [JsonPropertyName("Rules")] public List<WorkflowJsonRuleDto> Rules { get; set; } = [];
}

public sealed class WorkflowGlobalParameterDto
{
    [JsonPropertyName("Name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("Expression")] public string Expression { get; set; } = string.Empty;
}

public sealed class WorkflowJsonStepDto
{
    [JsonPropertyName("StepName")] public string StepName { get; set; } = string.Empty;
    [JsonPropertyName("DisplayOrder")] public int DisplayOrder { get; set; }
}

public sealed class WorkflowJsonParameterDto
{
    [JsonPropertyName("ParameterCode")] public string ParameterCode { get; set; } = string.Empty;
    [JsonPropertyName("ParameterName")] public string ParameterName { get; set; } = string.Empty;
    [JsonPropertyName("WhyItMatters")] public string WhyItMatters { get; set; } = string.Empty;
    [JsonPropertyName("HowDoWeMeasure")] public string HowDoWeMeasure { get; set; } = string.Empty;
    [JsonPropertyName("Weight")] public decimal Weight { get; set; }
    [JsonPropertyName("DisplayOrder")] public int DisplayOrder { get; set; }
    [JsonPropertyName("IsValueDriven")] public bool IsValueDriven { get; set; }
    [JsonPropertyName("Scores")] public List<WorkflowJsonScoreDto> Scores { get; set; } = [];
}

public sealed class WorkflowJsonScoreDto
{
    [JsonPropertyName("Score")] public int Score { get; set; }
    [JsonPropertyName("Definition")] public string Definition { get; set; } = string.Empty;
    [JsonPropertyName("MinValue")] public decimal? MinValue { get; set; }
    [JsonPropertyName("MaxValue")] public decimal? MaxValue { get; set; }
    [JsonPropertyName("MinInclusive")] public bool MinInclusive { get; set; } = true;
    [JsonPropertyName("MaxInclusive")] public bool MaxInclusive { get; set; } = true;
    [JsonPropertyName("ThresholdValue")] public decimal? ThresholdValue { get; set; }
    [JsonPropertyName("Unit")] public string? Unit { get; set; }
}

public sealed class WorkflowJsonRuleDto
{
    [JsonPropertyName("RuleName")] public string RuleName { get; set; } = string.Empty;
    [JsonPropertyName("SuccessEvent")] public string SuccessEvent { get; set; } = "Calculated";
    [JsonPropertyName("Expression")] public string Expression { get; set; } = "true";
    [JsonPropertyName("RuleExpressionType")] public string RuleExpressionType { get; set; } = "LambdaExpression";
    [JsonPropertyName("OutputExpression")] public string OutputExpression { get; set; } = string.Empty;
}

public sealed class SaveWorkflowDefinitionRequest
{
    public string WorkflowCode { get; set; } = string.Empty;
    public string VersionName { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public bool IsPublished { get; set; }
    public WorkflowDefinitionDto Definition { get; set; } = new();
}
