using CRISP.Application.DTOs;
using CRISP.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CRISP.Api.Controllers;

[ApiController]
[Route("api/framework")]
public sealed class FrameworkController(
    IFrameworkService frameworkService,
    IRuleEvaluationService evaluationService) : ControllerBase
{
    [HttpGet("workflows")]
    public async Task<ActionResult<IReadOnlyList<WorkflowDto>>> Workflows(CancellationToken ct) => Ok(await frameworkService.GetWorkflowsAsync(ct));

    [HttpGet("context/{code}")]
    public async Task<IActionResult> WorkflowContext(string code, CancellationToken ct)
    {
        var context = await frameworkService.GetWorkflowContextAsync(code, ct);
        return context is null ? NotFound() : Ok(context);
    }

    [HttpGet("entities")]
    public async Task<ActionResult<IReadOnlyList<EntityDto>>> Entities([FromQuery] string? search, CancellationToken ct) => Ok(await frameworkService.SearchEntitiesAsync(search, ct));

    [HttpGet("entities/{entityId:int}/frameworks")]
    public async Task<ActionResult<IReadOnlyList<FrameworkLookupDto>>> FrameworkOptions(int entityId, CancellationToken ct) =>
        Ok(await frameworkService.GetFrameworkOptionsAsync(entityId, ct));

    [HttpGet("{code}/steps")]
    public async Task<ActionResult<IReadOnlyList<FrameworkStepDto>>> Steps(string code, CancellationToken ct) => Ok(await frameworkService.GetStepsAsync(code, ct));

    [HttpGet("{code}/industries")]
    public async Task<ActionResult<IReadOnlyList<IndustrySegmentDto>>> Industries(string code, CancellationToken ct) => Ok(await frameworkService.GetIndustrySegmentsAsync(code, ct));

    [HttpGet("instances")]
    public async Task<ActionResult<IReadOnlyList<FrameworkListDto>>> Instances(CancellationToken ct) => Ok(await frameworkService.GetFrameworksAsync(ct));

    [HttpGet("{code}/parameters")]
    public async Task<ActionResult<IReadOnlyList<ParameterDto>>> Parameters(string code, CancellationToken ct) => Ok(await frameworkService.GetParametersAsync(code, ct));

    [HttpGet("{code}/versions")]
    public async Task<ActionResult<IReadOnlyList<VersionDto>>> Versions(string code, CancellationToken ct) => Ok(await frameworkService.GetVersionsAsync(code, ct));

    [HttpGet("{code}/versions/{versionNo:int}")]
    public async Task<IActionResult> Version(string code, int versionNo, CancellationToken ct)
    {
        var version = await frameworkService.GetVersionAsync(code, versionNo, ct);
        return version is null ? NotFound() : Ok(new { version.Id, version.VersionNo, version.VersionName, version.Status, version.IsPublished, version.CreatedOn, version.CreatedBy, version.JsonContent });
    }

    [HttpGet("instances/{id:int}")]
    public async Task<IActionResult> Instance(int id, CancellationToken ct)
    {
        var framework = await frameworkService.GetFrameworkAsync(id, ct);
        return framework is null ? NotFound() : Ok(framework);
    }

    [HttpPost("instances")]
    public async Task<ActionResult<FrameworkDto>> SaveInstance(FrameworkDto model, CancellationToken ct) => Ok(await frameworkService.SaveFrameworkAsync(model, ct));

    [HttpPost("calculate-score")]
    public async Task<IActionResult> CalculateScore(CalculateScoreRequest request, CancellationToken ct)
    {
        var result = await frameworkService.CalculateScoreAsync(request.ParameterId, request.Value, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("evaluate")]
    public async Task<ActionResult<EvaluateResponse>> Evaluate(EvaluateRequest request, CancellationToken ct) => Ok(await evaluationService.EvaluateAsync(request, ct));

    [HttpPost("versions")]
    public async Task<ActionResult<VersionDto>> SaveVersion(SaveVersionRequest request, CancellationToken ct) => Ok(await frameworkService.SaveVersionAsync(request, ct));

    [HttpGet("{code}/definition")]
    public async Task<IActionResult> Definition(string code, CancellationToken ct)
    {
        var definition = await frameworkService.GetWorkflowDefinitionAsync(code, ct);
        return definition is null ? NotFound() : Ok(definition);
    }

    [HttpPost("definition")]
    public async Task<ActionResult<VersionDto>> SaveDefinition(SaveWorkflowDefinitionRequest request, CancellationToken ct)
        => Ok(await frameworkService.SaveWorkflowDefinitionAsync(request, ct));
}
