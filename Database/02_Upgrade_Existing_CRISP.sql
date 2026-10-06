/* HISTORICAL/OBSOLETE - DO NOT RUN FOR THE CURRENT CRISP BUILD.
   The current EF Core model does not use WorkflowMaster.RouteKey or ActiveVersionNo. */
/* OBSOLETE FOR CURRENT CRISP BUILD: current EF model does NOT require RouteKey or ActiveVersionNo. */
USE CRISPDemo;
GO

-- This script upgrades the earlier CRISP database without deleting existing framework data.
IF COL_LENGTH('WorkflowMaster','RouteKey') IS NULL ALTER TABLE WorkflowMaster ADD RouteKey NVARCHAR(100) NULL;
UPDATE WorkflowMaster SET RouteKey = CASE WHEN WorkflowCode='CorporateBusiness' THEN 'corporate' ELSE LOWER(REPLACE(WorkflowCode,'Business','')) END WHERE RouteKey IS NULL;
ALTER TABLE WorkflowMaster ALTER COLUMN RouteKey NVARCHAR(100) NOT NULL;

IF COL_LENGTH('WorkflowMaster','ActiveVersionNo') IS NULL ALTER TABLE WorkflowMaster ADD ActiveVersionNo INT NULL;
UPDATE wm SET ActiveVersionNo = COALESCE(pub.VersionNo, anyVersion.VersionNo, 1)
FROM WorkflowMaster wm
OUTER APPLY (SELECT TOP (1) wj.VersionNo FROM WorkflowJsonVersion wj WHERE wj.WorkflowMasterId=wm.Id AND wj.IsPublished=1 ORDER BY wj.VersionNo DESC) pub
OUTER APPLY (SELECT TOP (1) wj.VersionNo FROM WorkflowJsonVersion wj WHERE wj.WorkflowMasterId=wm.Id ORDER BY wj.VersionNo DESC) anyVersion
WHERE ActiveVersionNo IS NULL OR ActiveVersionNo <= 0;
ALTER TABLE WorkflowMaster ALTER COLUMN ActiveVersionNo INT NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_WorkflowMaster_RouteKey') CREATE UNIQUE INDEX UX_WorkflowMaster_RouteKey ON WorkflowMaster(RouteKey);

IF OBJECT_ID('CustomerMaster','U') IS NULL
BEGIN
    CREATE TABLE CustomerMaster(Id INT IDENTITY(1,1) PRIMARY KEY,EntityName NVARCHAR(250) NOT NULL UNIQUE,IsActive BIT NOT NULL DEFAULT 1);
END;
IF OBJECT_ID('IndustryDetails','U') IS NULL
BEGIN
    CREATE TABLE IndustryDetails(Id INT IDENTITY(1,1) PRIMARY KEY,WorkflowMasterId INT NOT NULL,IndustryName NVARCHAR(250) NOT NULL,IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_IndustrySegment_Workflow_Upgrade FOREIGN KEY(WorkflowMasterId) REFERENCES WorkflowMaster(Id),
        CONSTRAINT UQ_IndustrySegment_Upgrade UNIQUE(WorkflowMasterId,IndustryName));
END;
IF COL_LENGTH('FrameworkInstance','EntityId') IS NULL ALTER TABLE FrameworkInstance ADD EntityId INT NULL;

INSERT INTO CustomerMaster(EntityName)
SELECT DISTINCT EntityName FROM FrameworkInstance f WHERE NOT EXISTS(SELECT 1 FROM CustomerMaster e WHERE e.EntityName=f.EntityName);
UPDATE f SET EntityId=e.Id FROM FrameworkInstance f JOIN CustomerMaster e ON e.EntityName=f.EntityName WHERE f.EntityId IS NULL;
ALTER TABLE FrameworkInstance ALTER COLUMN EntityId INT NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_FrameworkInstance_Entity_Upgrade') ALTER TABLE FrameworkInstance ADD CONSTRAINT FK_FrameworkInstance_Entity_Upgrade FOREIGN KEY(EntityId) REFERENCES CustomerMaster(Id);

IF OBJECT_ID('BusinessParameterDetails','U') IS NULL
BEGIN
    CREATE TABLE BusinessParameterDetails(Id INT IDENTITY(1,1) PRIMARY KEY,FrameworkInstanceId INT NOT NULL,IndustrySegmentId INT NOT NULL,Weight DECIMAL(10,2) NOT NULL DEFAULT 100,Status NVARCHAR(30) NOT NULL DEFAULT 'Saved',
        CONSTRAINT FK_BusinessParameterDetails_Framework_Upgrade FOREIGN KEY(FrameworkInstanceId) REFERENCES FrameworkInstance(Id) ON DELETE CASCADE,
        CONSTRAINT FK_BusinessParameterDetails_Industry_Upgrade FOREIGN KEY(IndustrySegmentId) REFERENCES IndustryDetails(Id));
END;

INSERT INTO IndustryDetails(WorkflowMasterId,IndustryName)
SELECT DISTINCT f.WorkflowMasterId,COALESCE(NULLIF(f.IndustryName,''),'Automobiles - Commercial Vehicles')
FROM FrameworkInstance f
WHERE NOT EXISTS(SELECT 1 FROM IndustryDetails i WHERE i.WorkflowMasterId=f.WorkflowMasterId AND i.IndustryName=COALESCE(NULLIF(f.IndustryName,''),'Automobiles - Commercial Vehicles'));

INSERT INTO BusinessParameterDetails(FrameworkInstanceId,IndustrySegmentId,Weight,Status)
SELECT f.Id,i.Id,100,'Saved' FROM FrameworkInstance f JOIN IndustryDetails i ON i.WorkflowMasterId=f.WorkflowMasterId AND i.IndustryName=COALESCE(NULLIF(f.IndustryName,''),'Automobiles - Commercial Vehicles')
WHERE NOT EXISTS(SELECT 1 FROM BusinessParameterDetails s WHERE s.FrameworkInstanceId=f.Id);

IF COL_LENGTH('ParameterDefinition','IsValueDriven') IS NULL ALTER TABLE ParameterDefinition ADD IsValueDriven BIT NOT NULL CONSTRAINT DF_ParameterDefinition_IsValueDriven_Upgrade DEFAULT 0;
IF COL_LENGTH('ScoreDefinition','MinValue') IS NULL ALTER TABLE ScoreDefinition ADD MinValue DECIMAL(18,4) NULL;
IF COL_LENGTH('ScoreDefinition','MaxValue') IS NULL ALTER TABLE ScoreDefinition ADD MaxValue DECIMAL(18,4) NULL;
IF COL_LENGTH('ScoreDefinition','MinInclusive') IS NULL ALTER TABLE ScoreDefinition ADD MinInclusive BIT NOT NULL CONSTRAINT DF_ScoreDefinition_MinInclusive_Upgrade DEFAULT 1;
IF COL_LENGTH('ScoreDefinition','MaxInclusive') IS NULL ALTER TABLE ScoreDefinition ADD MaxInclusive BIT NOT NULL CONSTRAINT DF_ScoreDefinition_MaxInclusive_Upgrade DEFAULT 1;

UPDATE ParameterDefinition SET IsValueDriven=0;
UPDATE ParameterDefinition SET IsValueDriven=1 WHERE ParameterCode IN('MarketPositionScore','ScaleScore');

UPDATE sd SET MinValue=30,MaxValue=NULL,MinInclusive=0,MaxInclusive=1 FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id=sd.ParameterDefinitionId WHERE p.ParameterCode='MarketPositionScore' AND sd.Score=1;
UPDATE sd SET MinValue=20,MaxValue=30,MinInclusive=1,MaxInclusive=1 FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id=sd.ParameterDefinitionId WHERE p.ParameterCode='MarketPositionScore' AND sd.Score=2;
UPDATE sd SET MinValue=10,MaxValue=20,MinInclusive=1,MaxInclusive=0 FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id=sd.ParameterDefinitionId WHERE p.ParameterCode='MarketPositionScore' AND sd.Score=3;
UPDATE sd SET MinValue=5,MaxValue=10,MinInclusive=1,MaxInclusive=0 FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id=sd.ParameterDefinitionId WHERE p.ParameterCode='MarketPositionScore' AND sd.Score=4;
UPDATE sd SET MinValue=NULL,MaxValue=5,MinInclusive=1,MaxInclusive=0 FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id=sd.ParameterDefinitionId WHERE p.ParameterCode='MarketPositionScore' AND sd.Score=5;

UPDATE sd SET MinValue=20000,MaxValue=NULL,MinInclusive=0,MaxInclusive=1 FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id=sd.ParameterDefinitionId WHERE p.ParameterCode='ScaleScore' AND sd.Score=1;
UPDATE sd SET MinValue=10000,MaxValue=20000,MinInclusive=1,MaxInclusive=1 FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id=sd.ParameterDefinitionId WHERE p.ParameterCode='ScaleScore' AND sd.Score=2;
UPDATE sd SET MinValue=5000,MaxValue=10000,MinInclusive=1,MaxInclusive=0 FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id=sd.ParameterDefinitionId WHERE p.ParameterCode='ScaleScore' AND sd.Score=3;
UPDATE sd SET MinValue=1000,MaxValue=5000,MinInclusive=1,MaxInclusive=0 FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id=sd.ParameterDefinitionId WHERE p.ParameterCode='ScaleScore' AND sd.Score=4;
UPDATE sd SET MinValue=NULL,MaxValue=1000,MinInclusive=1,MaxInclusive=0 FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id=sd.ParameterDefinitionId WHERE p.ParameterCode='ScaleScore' AND sd.Score=5;
GO
