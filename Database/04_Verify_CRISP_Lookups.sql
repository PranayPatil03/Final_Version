USE CRISPDemo;
GO

-- Safe verification/repair for the Entity -> Framework lookup flow.
-- This script does not drop or recreate existing framework data.

IF OBJECT_ID('dbo.CustomerMaster','U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomerMaster
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EntityName NVARCHAR(250) NOT NULL UNIQUE,
        IsActive BIT NOT NULL DEFAULT 1
    );
END
GO

-- Rebuild CustomerMaster entries from existing FrameworkInstance rows when needed.
INSERT INTO dbo.CustomerMaster(EntityName)
SELECT DISTINCT f.EntityName
FROM dbo.FrameworkInstance f
WHERE NULLIF(LTRIM(RTRIM(f.EntityName)), '') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.CustomerMaster c WHERE c.EntityName = f.EntityName);
GO

-- Existing rows may have been created by an earlier migration with IsActive = 0.
UPDATE dbo.CustomerMaster SET IsActive = 1 WHERE IsActive IS NULL OR IsActive = 0;
GO

-- Ensure the known demo customers exist only if they are absent.
IF NOT EXISTS (SELECT 1 FROM dbo.CustomerMaster WHERE EntityName='20 Microns Limited')
    INSERT INTO dbo.CustomerMaster(EntityName) VALUES ('20 Microns Limited');
IF NOT EXISTS (SELECT 1 FROM dbo.CustomerMaster WHERE EntityName='20 Microns Minerals, Foods Limited-Test 10')
    INSERT INTO dbo.CustomerMaster(EntityName) VALUES ('20 Microns Minerals, Foods Limited-Test 10');
IF NOT EXISTS (SELECT 1 FROM dbo.CustomerMaster WHERE EntityName='20 Microns Nano Minerals Limited')
    INSERT INTO dbo.CustomerMaster(EntityName) VALUES ('20 Microns Nano Minerals Limited');
GO

-- Link old framework rows to CustomerMaster where EntityId was missing.
IF COL_LENGTH('dbo.FrameworkInstance','EntityId') IS NOT NULL
BEGIN
    UPDATE f
       SET EntityId = c.Id
    FROM dbo.FrameworkInstance f
    INNER JOIN dbo.CustomerMaster c ON c.EntityName = f.EntityName
    WHERE f.EntityId IS NULL OR f.EntityId = 0;
END
GO

-- Check the lookup data. These two result sets should contain rows.
SELECT Id, EntityName, IsActive FROM dbo.CustomerMaster ORDER BY EntityName;
SELECT f.Id, f.EntityId, f.FrameworkName, f.Status, f.FrameworkDate, wm.WorkflowCode
FROM dbo.FrameworkInstance f
INNER JOIN dbo.WorkflowMaster wm ON wm.Id = f.WorkflowMasterId
ORDER BY f.FrameworkDate DESC, f.Id DESC;
GO
