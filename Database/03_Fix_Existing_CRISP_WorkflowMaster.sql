/* HISTORICAL/OBSOLETE - DO NOT RUN FOR THE CURRENT CRISP BUILD.
   The current EF Core model does not use WorkflowMaster.RouteKey or ActiveVersionNo. */
/* OBSOLETE FOR CURRENT CRISP BUILD: current EF model does NOT require RouteKey or ActiveVersionNo. */
USE CRISPDemo;
GO

/*
    CRISP database upgrade - WorkflowMaster compatibility fix.
    Run this script against an EXISTING CRISPDemo database before starting the API.

    The current EF Core model expects:
      WorkflowMaster.RouteKey
      WorkflowMaster.ActiveVersionNo

    Older CRISP databases did not contain these two columns, which caused:
      Invalid column name 'RouteKey'.
      Invalid column name 'ActiveVersionNo'.

    This script does not delete framework data.
*/

IF OBJECT_ID('dbo.WorkflowMaster', 'U') IS NULL
BEGIN
    THROW 50001, 'WorkflowMaster table was not found in CRISPDemo.', 1;
END;
GO

/* RouteKey */
IF COL_LENGTH('dbo.WorkflowMaster', 'RouteKey') IS NULL
BEGIN
    ALTER TABLE dbo.WorkflowMaster ADD RouteKey NVARCHAR(100) NULL;
END;
GO

/* Populate RouteKey only where it is missing. */
UPDATE dbo.WorkflowMaster
SET RouteKey = CASE
    WHEN WorkflowCode = 'CorporateBusiness' THEN 'corporate'
    WHEN WorkflowCode LIKE '%Business' THEN LOWER(REPLACE(WorkflowCode, 'Business', ''))
    ELSE LOWER(WorkflowCode)
END
WHERE NULLIF(LTRIM(RTRIM(RouteKey)), '') IS NULL;
GO

/* ActiveVersionNo */
IF COL_LENGTH('dbo.WorkflowMaster', 'ActiveVersionNo') IS NULL
BEGIN
    ALTER TABLE dbo.WorkflowMaster ADD ActiveVersionNo INT NULL;
END;
GO

/* Prefer the published JSON version. Fall back to version 1, then 1. */
UPDATE wm
SET ActiveVersionNo = COALESCE(pub.VersionNo, anyVersion.VersionNo, 1)
FROM dbo.WorkflowMaster wm
OUTER APPLY
(
    SELECT TOP (1) wj.VersionNo
    FROM dbo.WorkflowJsonVersion wj
    WHERE wj.WorkflowMasterId = wm.Id
      AND wj.IsPublished = 1
    ORDER BY wj.VersionNo DESC
) pub
OUTER APPLY
(
    SELECT TOP (1) wj.VersionNo
    FROM dbo.WorkflowJsonVersion wj
    WHERE wj.WorkflowMasterId = wm.Id
    ORDER BY wj.VersionNo DESC
) anyVersion
WHERE ActiveVersionNo IS NULL OR ActiveVersionNo <= 0;
GO

/* Make the new columns required after data has been populated. */
ALTER TABLE dbo.WorkflowMaster ALTER COLUMN RouteKey NVARCHAR(100) NOT NULL;
ALTER TABLE dbo.WorkflowMaster ALTER COLUMN ActiveVersionNo INT NOT NULL;
GO

/* Create the indexes only when they do not already exist. */
IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.WorkflowMaster')
      AND name = 'UX_WorkflowMaster_RouteKey'
)
BEGIN
    CREATE UNIQUE INDEX UX_WorkflowMaster_RouteKey
        ON dbo.WorkflowMaster(RouteKey);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.WorkflowMaster')
      AND name = 'UX_WorkflowMaster_WorkflowCode'
)
BEGIN
    CREATE UNIQUE INDEX UX_WorkflowMaster_WorkflowCode
        ON dbo.WorkflowMaster(WorkflowCode);
END;
GO

SELECT Id, WorkflowCode, RouteKey, WorkflowName, ActiveVersionNo, IsActive
FROM dbo.WorkflowMaster
ORDER BY Id;
GO
