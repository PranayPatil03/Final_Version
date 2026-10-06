USE CRISPDemo;
GO

-- Verify the exact lookup data used by the UI.
SELECT Id, EntityName, IsActive
FROM dbo.CustomerMaster
WHERE IsActive = 1
ORDER BY EntityName;

SELECT
    f.Id,
    f.EntityId,
    f.FrameworkName,
    f.Status,
    f.FrameworkDate,
    f.WorkflowVersionNo,
    f.IndustryName,
    w.WorkflowCode,
    w.WorkflowName
FROM dbo.FrameworkInstance f
INNER JOIN dbo.WorkflowMaster w ON w.Id = f.WorkflowMasterId
ORDER BY f.FrameworkDate DESC, f.Id DESC;

SELECT
    w.Id, w.WorkflowCode, w.WorkflowName, w.IsActive,
    v.VersionNo, v.VersionName, v.Status, v.IsPublished
FROM dbo.WorkflowMaster w
LEFT JOIN dbo.WorkflowJsonVersion v ON v.WorkflowMasterId = w.Id
WHERE w.IsActive = 1
ORDER BY w.Id, v.VersionNo DESC;
GO
