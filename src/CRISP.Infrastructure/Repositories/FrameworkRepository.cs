using System.Text.Json;
using CRISP.Application.DTOs;
using CRISP.Application.Interfaces;
using CRISP.Domain.Entities;
using CRISP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Infrastructure.Repositories;

public sealed class FrameworkRepository(AppDbContext db) : IFrameworkRepository
{
    public async Task<IReadOnlyList<WorkflowDto>> GetWorkflowsAsync(CancellationToken ct = default) =>
        await db.WorkflowMasters.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.WorkflowName)
            .Select(x => new WorkflowDto(x.Id, x.WorkflowCode, x.WorkflowName, x.IsActive)).ToListAsync(ct);

    public async Task<WorkflowContextDto?> GetWorkflowContextAsync(string workflowCode, CancellationToken ct = default)
    {
        var workflow = await db.WorkflowMasters.AsNoTracking()
            .Where(x => x.WorkflowCode == workflowCode && x.IsActive)
            .Select(x => new WorkflowDto(x.Id, x.WorkflowCode, x.WorkflowName, x.IsActive))
            .FirstOrDefaultAsync(ct);

        if (workflow is null)
            return null;

        var steps = await db.WorkflowSteps.AsNoTracking()
            .Where(x => x.WorkflowMaster.WorkflowCode == workflowCode && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new FrameworkStepDto(x.Id, x.StepName, x.DisplayOrder, x.IsActive))
            .ToListAsync(ct);

        return new WorkflowContextDto
        {
            Workflow = workflow,
            Steps = steps
        };
    }

    public async Task<IReadOnlyList<EntityDto>> SearchEntitiesAsync(string? search, CancellationToken ct = default)
    {
        var term = search?.Trim() ?? string.Empty;

        // Primary source: CustomerMaster.
        // This is the only source used for the entity identity shown to the UI.
        var customerQuery = db.CustomerMasters.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(term))
            customerQuery = customerQuery.Where(x => x.EntityName.Contains(term));

        var customers = await customerQuery
            .OrderBy(x => x.EntityName)
            .Take(20)
            .Select(x => new EntityDto(x.Id, x.EntityName))
            .ToListAsync(ct);

        if (customers.Count > 0)
            return customers;

        // Compatibility fallback for databases upgraded from the older CRISP schema.
        // Older FrameworkInstance rows may contain the entity even when CustomerMaster
        // was not populated/linked yet. This keeps the lookup data-driven and avoids
        // hardcoding company names in the UI.
        var frameworkQuery = db.FrameworkInstances.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(term))
            frameworkQuery = frameworkQuery.Where(x => x.EntityName.Contains(term));

        return await frameworkQuery
            .Where(x => x.EntityId > 0)
            .GroupBy(x => new { x.EntityId, x.EntityName })
            .OrderBy(x => x.Key.EntityName)
            .Take(20)
            .Select(x => new EntityDto(x.Key.EntityId, x.Key.EntityName))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FrameworkLookupDto>> GetFrameworkOptionsAsync(int entityId, CancellationToken ct = default)
    {
        return await db.FrameworkInstances.AsNoTracking()
            .Where(x => x.EntityId == entityId)
            .OrderByDescending(x => x.FrameworkDate).ThenByDescending(x => x.Id)
            .Select(x => new FrameworkLookupDto(
                x.Id,
                x.FrameworkName,
                x.Status,
                x.FrameworkDate,
                x.WorkflowVersionNo,
                x.IndustrySegments.OrderBy(s => s.Id).Select(s => s.IndustrySegment.IndustryName).FirstOrDefault() ?? x.IndustryName))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FrameworkStepDto>> GetStepsAsync(string workflowCode, CancellationToken ct = default) =>
        await db.WorkflowSteps.AsNoTracking().Where(x => x.WorkflowMaster.WorkflowCode == workflowCode && x.IsActive).OrderBy(x => x.DisplayOrder)
            .Select(x => new FrameworkStepDto(x.Id, x.StepName, x.DisplayOrder, x.IsActive)).ToListAsync(ct);

    public async Task<IReadOnlyList<IndustrySegmentDto>> GetIndustrySegmentsAsync(string workflowCode, CancellationToken ct = default) =>
        await db.IndustryDetails.AsNoTracking()
            .Where(x => x.WorkflowMaster.WorkflowCode == workflowCode && x.IsActive)
            .OrderBy(x => x.IndustryName)
            .Select(x => new IndustrySegmentDto(x.Id, x.IndustryName)).ToListAsync(ct);

    public async Task<IReadOnlyList<FrameworkListDto>> GetFrameworksAsync(CancellationToken ct = default) =>
        await db.FrameworkInstances.AsNoTracking().OrderByDescending(x => x.Id)
            .Select(x => new FrameworkListDto(x.Id, x.Entity.EntityName, x.IndustryName, x.FrameworkName, x.Status, x.WorkflowVersionNo, x.FrameworkDate)).ToListAsync(ct);

    public async Task<WorkflowMaster?> GetWorkflowAsync(string code, CancellationToken ct = default) =>
        await db.WorkflowMasters.FirstOrDefaultAsync(x => x.WorkflowCode == code && x.IsActive, ct);

    public async Task<ParameterDto?> GetParameterAsync(int parameterId, CancellationToken ct = default)
    {
        return await db.ParameterDefinitions.AsNoTracking().Include(x => x.Scores).Where(x => x.Id == parameterId && x.IsActive)
            .Select(x => new ParameterDto(x.Id, x.ParameterCode, x.ParameterName, x.WhyItMatters, x.HowDoWeMeasure, x.Weight, x.DisplayOrder, x.IsValueDriven,
                x.Scores.OrderBy(s => s.Score).Select(s => new ScoreDto(s.Score, s.Definition, s.ThresholdValue, s.Unit, s.MinValue, s.MaxValue, s.MinInclusive, s.MaxInclusive)).ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<ParameterDto>> GetParametersAsync(string code, CancellationToken ct = default)
    {
        var workflow = await db.WorkflowMasters.AsNoTracking().FirstOrDefaultAsync(x => x.WorkflowCode == code && x.IsActive, ct);
        if (workflow is null) return [];
        return await db.ParameterDefinitions.AsNoTracking().Include(x => x.Scores)
            .Where(x => x.WorkflowMasterId == workflow.Id && x.IsActive).OrderBy(x => x.DisplayOrder)
            .Select(x => new ParameterDto(x.Id, x.ParameterCode, x.ParameterName, x.WhyItMatters, x.HowDoWeMeasure, x.Weight, x.DisplayOrder, x.IsValueDriven,
                x.Scores.OrderBy(s => s.Score).Select(s => new ScoreDto(s.Score, s.Definition, s.ThresholdValue, s.Unit, s.MinValue, s.MaxValue, s.MinInclusive, s.MaxInclusive)).ToList()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VersionDto>> GetVersionsAsync(string code, CancellationToken ct = default) =>
        await db.WorkflowJsonVersions.AsNoTracking().Where(x => x.WorkflowMaster.WorkflowCode == code).OrderByDescending(x => x.VersionNo)
            .Select(x => new VersionDto(x.Id, x.VersionNo, x.VersionName, x.Status, x.IsPublished, x.CreatedOn, x.CreatedBy)).ToListAsync(ct);

    public async Task<WorkflowJsonVersion?> GetVersionAsync(string code, int versionNo, CancellationToken ct = default) =>
        await db.WorkflowJsonVersions.Include(x => x.WorkflowMaster).FirstOrDefaultAsync(x => x.WorkflowMaster.WorkflowCode == code && x.VersionNo == versionNo, ct);

    public async Task<FrameworkDto?> GetFrameworkAsync(int id, CancellationToken ct = default)
    {
        var item = await db.FrameworkInstances.AsNoTracking()
            .Include(x => x.Entity)
            .Include(x => x.WorkflowMaster).ThenInclude(x => x.Steps)
            .Include(x => x.ParameterValues).ThenInclude(x => x.ParameterDefinition)
            .Include(x => x.IndustrySegments).ThenInclude(x => x.IndustrySegment)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return null;
        var segments = item.IndustrySegments.OrderBy(x => x.Id)
            .Select(x => new BusinessParameterDetailsDto(x.Id, x.IndustrySegmentId, x.IndustrySegment.IndustryName, x.Weight, x.Status)).ToList();

        // Compatibility for existing framework rows created before BusinessParameterDetails
        // was populated. The framework row still contains IndustryName, so resolve the
        // matching master record and expose it to the UI as a real industry segment.
        if (segments.Count == 0 && !string.IsNullOrWhiteSpace(item.IndustryName))
        {
            var legacyIndustry = await db.IndustryDetails.AsNoTracking()
                .Where(x => x.WorkflowMasterId == item.WorkflowMasterId
                         && x.IsActive
                         && x.IndustryName == item.IndustryName)
                .Select(x => new { x.Id, x.IndustryName })
                .FirstOrDefaultAsync(ct);

            if (legacyIndustry is not null)
            {
                segments.Add(new BusinessParameterDetailsDto(
                    0,
                    legacyIndustry.Id,
                    legacyIndustry.IndustryName,
                    100,
                    item.Status));
            }
        }

        var industry = segments.FirstOrDefault()?.IndustryName ?? item.IndustryName;
        var parameterValues = item.ParameterValues.OrderBy(x => x.ParameterDefinition.DisplayOrder)
            .Select(x => new FrameworkParameterDto(x.ParameterDefinitionId, x.ParameterDefinition.ParameterName, x.ParameterDefinition.Weight, x.Value, x.Score)).ToList();

        var parameterDefinitions = await db.ParameterDefinitions.AsNoTracking()
            .Include(x => x.Scores)
            .Where(x => x.WorkflowMasterId == item.WorkflowMasterId && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new ParameterDto(x.Id, x.ParameterCode, x.ParameterName, x.WhyItMatters, x.HowDoWeMeasure, x.Weight, x.DisplayOrder, x.IsValueDriven,
                x.Scores.OrderBy(s => s.Score).Select(s => new ScoreDto(s.Score, s.Definition, s.ThresholdValue, s.Unit, s.MinValue, s.MaxValue, s.MinInclusive, s.MaxInclusive)).ToList()))
            .ToListAsync(ct);

        return new FrameworkDto
        {
            Id = item.Id,
            EntityId = item.EntityId,
            WorkflowCode = item.WorkflowMaster.WorkflowCode,
            EntityName = item.Entity.EntityName,
            IndustryName = industry,
            FrameworkName = item.FrameworkName,
            Status = item.Status,
            WorkflowVersionNo = item.WorkflowVersionNo,
            FrameworkDate = item.FrameworkDate,
            ConsolidatedView = item.ConsolidatedView,
            Steps = item.WorkflowMaster.Steps.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder)
                .Select(x => new FrameworkStepDto(x.Id, x.StepName, x.DisplayOrder, x.IsActive)).ToList(),
            IndustrySegments = segments,
            ParameterValues = parameterValues,
            Parameters = parameterDefinitions,
            Workflow = new WorkflowSummaryDto
            {
                Id = item.WorkflowMaster.Id,
                WorkflowCode = item.WorkflowMaster.WorkflowCode,
                WorkflowName = item.WorkflowMaster.WorkflowName,
                IsActive = item.WorkflowMaster.IsActive
            }
        };
    }

    public async Task<FrameworkDto> SaveFrameworkAsync(FrameworkDto framework, CancellationToken ct = default)
    {
        var workflow = await db.WorkflowMasters.FirstOrDefaultAsync(x => x.WorkflowCode == framework.WorkflowCode && x.IsActive, ct)
            ?? throw new InvalidOperationException("Workflow not found.");
        var customerMaster = await db.CustomerMasters.FirstOrDefaultAsync(x => x.Id == framework.EntityId, ct)
            ?? throw new InvalidOperationException("Entity not found.");

        FrameworkInstance? entity = framework.Id > 0
            ? await db.FrameworkInstances.Include(x => x.ParameterValues).Include(x => x.IndustrySegments).FirstOrDefaultAsync(x => x.Id == framework.Id, ct)
            : null;

        if (entity is null)
        {
            entity = new FrameworkInstance
            {
                WorkflowMasterId = workflow.Id,
                EntityId = customerMaster.Id,
                EntityName = customerMaster.EntityName,
                IndustryName = framework.IndustryName,
                FrameworkName = framework.FrameworkName,
                Status = framework.Status,
                WorkflowVersionNo = framework.WorkflowVersionNo,
                FrameworkDate = framework.FrameworkDate == default ? DateTime.Today : framework.FrameworkDate,
                ConsolidatedView = framework.ConsolidatedView
            };
            db.FrameworkInstances.Add(entity);
        }
        else
        {
            entity.WorkflowMasterId = workflow.Id;
            entity.EntityId = customerMaster.Id;
            entity.EntityName = customerMaster.EntityName;
            entity.IndustryName = framework.IndustryName;
            entity.FrameworkName = framework.FrameworkName;
            entity.Status = framework.Status;
            entity.WorkflowVersionNo = framework.WorkflowVersionNo;
            entity.ConsolidatedView = framework.ConsolidatedView;
            db.FrameworkParameterValues.RemoveRange(entity.ParameterValues);
            db.BusinessParameterDetails.RemoveRange(entity.IndustrySegments);
        }

        foreach (var segment in framework.IndustrySegments)
        {
            entity.IndustrySegments.Add(new BusinessParameterDetails
            {
                IndustrySegmentId = segment.IndustrySegmentId,
                Weight = segment.Weight,
                Status = segment.Status
            });
        }

        foreach (var p in framework.ParameterValues)
            entity.ParameterValues.Add(new FrameworkParameterValue { ParameterDefinitionId = p.ParameterId, Value = p.Value, Score = p.Score });

        await db.SaveChangesAsync(ct);
        return (await GetFrameworkAsync(entity.Id, ct))!;
    }

    public async Task<VersionDto> SaveVersionAsync(SaveVersionRequest request, CancellationToken ct = default)
    {
        var workflow = await db.WorkflowMasters.FirstOrDefaultAsync(x => x.WorkflowCode == request.WorkflowCode, ct)
            ?? throw new InvalidOperationException("Workflow not found.");
        if (request.VersionNo <= 0)
            request.VersionNo = (await db.WorkflowJsonVersions.Where(x => x.WorkflowMasterId == workflow.Id).MaxAsync(x => (int?)x.VersionNo, ct) ?? 0) + 1;
        var version = await db.WorkflowJsonVersions.FirstOrDefaultAsync(x => x.WorkflowMasterId == workflow.Id && x.VersionNo == request.VersionNo, ct);
        if (version is null)
        {
            version = new WorkflowJsonVersion { WorkflowMasterId = workflow.Id, VersionNo = request.VersionNo };
            db.WorkflowJsonVersions.Add(version);
        }
        version.VersionName = request.VersionName;
        version.Status = request.Status;
        version.JsonContent = request.JsonContent;
        version.CreatedBy = "Admin";
        version.CreatedOn = DateTime.UtcNow;
        version.IsPublished = request.IsPublished;
        if (request.IsPublished)
        {
            await db.WorkflowJsonVersions.Where(x => x.WorkflowMasterId == workflow.Id && x.Id != version.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsPublished, false), ct);
        }
        await db.SaveChangesAsync(ct);
        return new VersionDto(version.Id, version.VersionNo, version.VersionName, version.Status, version.IsPublished, version.CreatedOn, version.CreatedBy);
    }

    public async Task<WorkflowDefinitionDto?> GetWorkflowDefinitionAsync(string workflowCode, CancellationToken ct = default)
    {
        var workflow = await db.WorkflowMasters.AsNoTracking().FirstOrDefaultAsync(x => x.WorkflowCode == workflowCode && x.IsActive, ct);
        if (workflow is null) return null;
        var version = await db.WorkflowJsonVersions.AsNoTracking()
            .Where(x => x.WorkflowMasterId == workflow.Id)
            .OrderByDescending(x => x.VersionNo)
            .FirstOrDefaultAsync(ct);
        if (version is null || string.IsNullOrWhiteSpace(version.JsonContent)) return new WorkflowDefinitionDto { WorkflowName = workflow.WorkflowName };
        try
        {
            using var doc = JsonDocument.Parse(version.JsonContent);
            var item = doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0
                ? doc.RootElement[0].Deserialize<WorkflowDefinitionDto>()
                : null;
            return item ?? new WorkflowDefinitionDto { WorkflowName = workflow.WorkflowName };
        }
        catch
        {
            return new WorkflowDefinitionDto { WorkflowName = workflow.WorkflowName };
        }
    }

    public async Task<VersionDto> SaveWorkflowDefinitionAsync(
        SaveWorkflowDefinitionRequest request,
        string jsonContent,
        CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        try
        {
            var workflow = await db.WorkflowMasters
                .FirstOrDefaultAsync(
                    x => x.WorkflowCode == request.WorkflowCode && x.IsActive,
                    ct)
                ?? throw new InvalidOperationException("Workflow not found.");

            var nextVersion = (await db.WorkflowJsonVersions
                .Where(x => x.WorkflowMasterId == workflow.Id)
                .MaxAsync(x => (int?)x.VersionNo, ct) ?? 0) + 1;

            var version = new WorkflowJsonVersion
            {
                WorkflowMasterId = workflow.Id,
                VersionNo = nextVersion,
                VersionName = string.IsNullOrWhiteSpace(request.VersionName)
                    ? $"{DateTime.Today:dd-MMM-yyyy} - Draft"
                    : request.VersionName,
                Status = request.IsPublished ? "Published" : "Draft",
                JsonContent = jsonContent,
                CreatedBy = "Admin",
                CreatedOn = DateTime.UtcNow,
                IsPublished = request.IsPublished
            };

            db.WorkflowJsonVersions.Add(version);

            // Parameter definitions are the relational read model of the JSON definition.
            var existingParameters = await db.ParameterDefinitions
                .Include(x => x.Scores)
                .Where(x => x.WorkflowMasterId == workflow.Id)
                .ToListAsync(ct);

            var activeParameterCodes = request.Definition.Parameters
                .Select(x => x.ParameterCode)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var definition in request.Definition.Parameters.OrderBy(x => x.DisplayOrder))
            {
                var parameter = existingParameters.FirstOrDefault(
                    x => x.ParameterCode.Equals(
                        definition.ParameterCode,
                        StringComparison.OrdinalIgnoreCase));

                if (parameter is null)
                {
                    parameter = new ParameterDefinition
                    {
                        WorkflowMasterId = workflow.Id,
                        ParameterCode = definition.ParameterCode
                    };
                    db.ParameterDefinitions.Add(parameter);
                    existingParameters.Add(parameter);
                }

                parameter.ParameterName = definition.ParameterName;
                parameter.WhyItMatters = definition.WhyItMatters;
                parameter.HowDoWeMeasure = definition.HowDoWeMeasure;
                parameter.Weight = definition.Weight;
                parameter.DisplayOrder = definition.DisplayOrder;
                parameter.IsValueDriven = definition.IsValueDriven;
                parameter.IsActive = true;

                var existingScores = parameter.Scores.ToList();
                var activeScores = definition.Scores.Select(x => x.Score).ToHashSet();

                foreach (var scoreDefinition in definition.Scores)
                {
                    var score = existingScores.FirstOrDefault(
                        x => x.Score == scoreDefinition.Score);

                    if (score is null)
                    {
                        score = new ScoreDefinition
                        {
                            ParameterDefinition = parameter,
                            Score = scoreDefinition.Score
                        };
                        db.ScoreDefinitions.Add(score);
                    }

                    score.Definition = scoreDefinition.Definition;
                    score.MinValue = scoreDefinition.MinValue;
                    score.MaxValue = scoreDefinition.MaxValue;
                    score.MinInclusive = scoreDefinition.MinInclusive;
                    score.MaxInclusive = scoreDefinition.MaxInclusive;
                    score.ThresholdValue = scoreDefinition.ThresholdValue;
                    score.Unit = scoreDefinition.Unit;
                }

                foreach (var obsoleteScore in existingScores
                    .Where(x => !activeScores.Contains(x.Score)))
                {
                    db.ScoreDefinitions.Remove(obsoleteScore);
                }
            }

            foreach (var obsoleteParameter in existingParameters
                .Where(x => !activeParameterCodes.Contains(x.ParameterCode)))
            {
                obsoleteParameter.IsActive = false;
            }

            // Workflow steps are also synchronized from JSON.
            var existingSteps = await db.WorkflowSteps
                .Where(x => x.WorkflowMasterId == workflow.Id)
                .ToListAsync(ct);

            var activeStepOrders = request.Definition.Steps
                .Select(x => x.DisplayOrder)
                .ToHashSet();

            foreach (var stepDefinition in request.Definition.Steps.OrderBy(x => x.DisplayOrder))
            {
                var step = existingSteps.FirstOrDefault(
                    x => x.DisplayOrder == stepDefinition.DisplayOrder);

                if (step is null)
                {
                    step = new WorkflowStep
                    {
                        WorkflowMasterId = workflow.Id,
                        DisplayOrder = stepDefinition.DisplayOrder
                    };
                    db.WorkflowSteps.Add(step);
                }

                step.StepName = stepDefinition.StepName;
                step.IsActive = true;
            }

            foreach (var obsoleteStep in existingSteps
                .Where(x => !activeStepOrders.Contains(x.DisplayOrder)))
            {
                obsoleteStep.IsActive = false;
            }

            // Rules are a first-class relational read model as well as part of the JSON snapshot.
            var existingRules = await db.RuleDefinitions
                .Where(x => x.WorkflowMasterId == workflow.Id)
                .ToListAsync(ct);

            var activeRuleNames = request.Definition.Rules
                .Select(x => x.RuleName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var ruleOrder = 1;
            foreach (var ruleDefinition in request.Definition.Rules)
            {
                var rule = existingRules.FirstOrDefault(
                    x => x.RuleName.Equals(
                        ruleDefinition.RuleName,
                        StringComparison.OrdinalIgnoreCase));

                if (rule is null)
                {
                    rule = new RuleDefinition
                    {
                        WorkflowMasterId = workflow.Id,
                        RuleName = ruleDefinition.RuleName
                    };
                    db.RuleDefinitions.Add(rule);
                }

                rule.SuccessEvent = ruleDefinition.SuccessEvent;
                rule.Expression = ruleDefinition.Expression;
                rule.RuleExpressionType = ruleDefinition.RuleExpressionType;
                rule.OutputExpression = ruleDefinition.OutputExpression;
                rule.DisplayOrder = ruleOrder++;
                rule.IsActive = true;
            }

            foreach (var obsoleteRule in existingRules
                .Where(x => !activeRuleNames.Contains(x.RuleName)))
            {
                obsoleteRule.IsActive = false;
            }

            if (request.IsPublished)
            {
                await db.WorkflowJsonVersions
                    .Where(x => x.WorkflowMasterId == workflow.Id && x.Id != version.Id)
                    .ExecuteUpdateAsync(
                        query => query.SetProperty(x => x.IsPublished, false)
                                         .SetProperty(x => x.Status, "Archived"),
                        ct);
            }

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return new VersionDto(
                version.Id,
                version.VersionNo,
                version.VersionName,
                version.Status,
                version.IsPublished,
                version.CreatedOn,
                version.CreatedBy);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

}