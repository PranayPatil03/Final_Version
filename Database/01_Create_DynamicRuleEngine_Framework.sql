IF DB_ID('CRISPDemo') IS NULL CREATE DATABASE CRISPDemo;
GO
USE CRISPDemo;
GO

DROP TABLE IF EXISTS FrameworkParameterValue;
DROP TABLE IF EXISTS BusinessParameterDetails;
DROP TABLE IF EXISTS FrameworkInstance;
DROP TABLE IF EXISTS ScoreDefinition;
DROP TABLE IF EXISTS ParameterDefinition;
DROP TABLE IF EXISTS IndustryDetails;
DROP TABLE IF EXISTS CustomerMaster;
DROP TABLE IF EXISTS WorkflowStep;
DROP TABLE IF EXISTS WorkflowJsonVersion;
DROP TABLE IF EXISTS WorkflowMaster;
GO

CREATE TABLE WorkflowMaster
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowCode NVARCHAR(100) NOT NULL UNIQUE,
    WorkflowName NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE WorkflowStep
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowMasterId INT NOT NULL,
    StepName NVARCHAR(150) NOT NULL,
    DisplayOrder INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_WorkflowStep_Workflow FOREIGN KEY (WorkflowMasterId) REFERENCES WorkflowMaster(Id),
    CONSTRAINT UQ_WorkflowStep UNIQUE(WorkflowMasterId, DisplayOrder)
);

CREATE TABLE WorkflowJsonVersion
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowMasterId INT NOT NULL,
    VersionNo INT NOT NULL,
    VersionName NVARCHAR(200) NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    JsonContent NVARCHAR(MAX) NOT NULL,
    CreatedOn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    IsPublished BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_WorkflowJsonVersion_Workflow FOREIGN KEY (WorkflowMasterId) REFERENCES WorkflowMaster(Id),
    CONSTRAINT UQ_WorkflowJsonVersion UNIQUE(WorkflowMasterId, VersionNo)
);

CREATE TABLE CustomerMaster
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EntityName NVARCHAR(250) NOT NULL UNIQUE,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE IndustryDetails
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowMasterId INT NOT NULL,
    IndustryName NVARCHAR(250) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_IndustrySegment_Workflow FOREIGN KEY (WorkflowMasterId) REFERENCES WorkflowMaster(Id),
    CONSTRAINT UQ_IndustrySegment UNIQUE(WorkflowMasterId, IndustryName)
);

CREATE TABLE ParameterDefinition
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowMasterId INT NOT NULL,
    ParameterCode NVARCHAR(100) NOT NULL,
    ParameterName NVARCHAR(250) NOT NULL,
    WhyItMatters NVARCHAR(MAX) NULL,
    HowDoWeMeasure NVARCHAR(MAX) NULL,
    Weight DECIMAL(10,2) NOT NULL,
    DisplayOrder INT NOT NULL,
    IsValueDriven BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_ParameterDefinition_Workflow FOREIGN KEY (WorkflowMasterId) REFERENCES WorkflowMaster(Id),
    CONSTRAINT UQ_ParameterDefinition UNIQUE (WorkflowMasterId, ParameterCode)
);

CREATE TABLE ScoreDefinition
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ParameterDefinitionId INT NOT NULL,
    Score INT NOT NULL,
    Definition NVARCHAR(MAX) NOT NULL,
    ThresholdValue DECIMAL(18,4) NULL,
    MinValue DECIMAL(18,4) NULL,
    MaxValue DECIMAL(18,4) NULL,
    MinInclusive BIT NOT NULL DEFAULT 1,
    MaxInclusive BIT NOT NULL DEFAULT 1,
    Unit NVARCHAR(100) NULL,
    RowNo INT NOT NULL DEFAULT 1,
    ColNo INT NOT NULL,
    CONSTRAINT FK_ScoreDefinition_Parameter FOREIGN KEY (ParameterDefinitionId) REFERENCES ParameterDefinition(Id),
    CONSTRAINT UQ_ScoreDefinition UNIQUE(ParameterDefinitionId, Score)
);

CREATE TABLE FrameworkInstance
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowMasterId INT NOT NULL,
    EntityId INT NOT NULL,
    EntityName NVARCHAR(250) NOT NULL,
    IndustryName NVARCHAR(250) NULL,
    FrameworkName NVARCHAR(250) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    WorkflowVersionNo INT NOT NULL,
    FrameworkDate DATE NOT NULL,
    ConsolidatedView BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_FrameworkInstance_Workflow FOREIGN KEY (WorkflowMasterId) REFERENCES WorkflowMaster(Id),
    CONSTRAINT FK_FrameworkInstance_Entity FOREIGN KEY (EntityId) REFERENCES CustomerMaster(Id)
);

CREATE TABLE BusinessParameterDetails
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FrameworkInstanceId INT NOT NULL,
    IndustrySegmentId INT NOT NULL,
    Weight DECIMAL(10,2) NOT NULL DEFAULT 100,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Saved',
    CONSTRAINT FK_BusinessParameterDetails_Framework FOREIGN KEY (FrameworkInstanceId) REFERENCES FrameworkInstance(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BusinessParameterDetails_Industry FOREIGN KEY (IndustrySegmentId) REFERENCES IndustryDetails(Id)
);

CREATE TABLE FrameworkParameterValue
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FrameworkInstanceId INT NOT NULL,
    ParameterDefinitionId INT NOT NULL,
    Value DECIMAL(18,4) NULL,
    Score INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_FrameworkParameterValue_Framework FOREIGN KEY (FrameworkInstanceId) REFERENCES FrameworkInstance(Id) ON DELETE CASCADE,
    CONSTRAINT FK_FrameworkParameterValue_Parameter FOREIGN KEY (ParameterDefinitionId) REFERENCES ParameterDefinition(Id)
);
GO

INSERT INTO WorkflowMaster(WorkflowCode,WorkflowName,IsActive) VALUES ('CorporateBusiness','Corporate Business Framework',1);
DECLARE @WorkflowId INT=SCOPE_IDENTITY();

INSERT INTO WorkflowStep(WorkflowMasterId,StepName,DisplayOrder) VALUES
(@WorkflowId,'Business',1),(@WorkflowId,'General',2),(@WorkflowId,'Others',3),(@WorkflowId,'Parent/Group',4),
(@WorkflowId,'Output',5),(@WorkflowId,'Sensitivity',6),(@WorkflowId,'Summary',7),(@WorkflowId,'Benchmarks',8);

INSERT INTO CustomerMaster(EntityName) VALUES
('20 Microns Limited'),
('20 Microns Minerals, Foods Limited-Test 10'),
('20 Microns Nano Minerals Limited');

INSERT INTO IndustryDetails(WorkflowMasterId,IndustryName) VALUES
(@WorkflowId,'Automobiles - Commercial Vehicles'),
(@WorkflowId,'Automobiles - Passenger Vehicles'),
(@WorkflowId,'Commercial Vehicles - Components');

INSERT INTO ParameterDefinition(WorkflowMasterId,ParameterCode,ParameterName,WhyItMatters,HowDoWeMeasure,Weight,DisplayOrder,IsValueDriven) VALUES
(@WorkflowId,'MarketPositionScore','Market Position & Brand Strength','Market share is a good proxy to a company''s scale & market position and R&D strengths','Weighted average market share across key commercial vehicle segments.',15,1,1),
(@WorkflowId,'ScaleScore','Scale','The scale of operations, as measured by revenues, production capacity and production output, is one of the primary factors in evaluating business position.','Revenue from commercial vehicle operations (Rs. Crore).',15,2,1),
(@WorkflowId,'ProductPortfolioScore','Product Portfolio','A strong product portfolio is essential for an OEM to sustain its competitive market position and cater to a diverse customer profile.','Product portfolio across M&HCV, LCV and Buses and tonnage-wise presence.',25,3,0),
(@WorkflowId,'GeographicDiversificationScore','Geographic Diversification','Geographic diversification helps mitigate risks arising from demand slowdown in a particular geography.','Domestic regional mix and exports across North, West, East and South.',25,4,0),
(@WorkflowId,'TechnologyScore','Technology & Product Development Capabilities','OEMs need to continuously refresh their offering to keep pace with changing technology and emission norms.','R&D spend and product development capabilities.',20,5,0);

DECLARE @P INT;
SELECT @P=Id FROM ParameterDefinition WHERE WorkflowMasterId=@WorkflowId AND ParameterCode='MarketPositionScore';
INSERT INTO ScoreDefinition(ParameterDefinitionId,Score,Definition,MinValue,MaxValue,MinInclusive,MaxInclusive,ThresholdValue,Unit,ColNo) VALUES
(@P,1,'Market share > 30%',30,NULL,0,1,30,'% ',1),(@P,2,'Market share 20-30%',20,30,1,1,20,'% ',2),(@P,3,'Market share 10-20%',10,20,1,0,10,'% ',3),(@P,4,'Market share 5-10%',5,10,1,0,5,'% ',4),(@P,5,'Market share below 5%',NULL,5,1,0,0,'% ',5);

SELECT @P=Id FROM ParameterDefinition WHERE WorkflowMasterId=@WorkflowId AND ParameterCode='ScaleScore';
INSERT INTO ScoreDefinition(ParameterDefinitionId,Score,Definition,MinValue,MaxValue,MinInclusive,MaxInclusive,ThresholdValue,Unit,ColNo) VALUES
(@P,1,'Revenue > 20,000',20000,NULL,0,1,20000,'Rs. Crore',1),(@P,2,'Revenue 10,000-20,000',10000,20000,1,1,10000,'Rs. Crore',2),(@P,3,'Revenue 5,000-10,000',5000,10000,1,0,5000,'Rs. Crore',3),(@P,4,'Revenue 1,000-5,000',1000,5000,1,0,1000,'Rs. Crore',4),(@P,5,'Revenue < 1,000',NULL,1000,1,0,0,'Rs. Crore',5);

SELECT @P=Id FROM ParameterDefinition WHERE WorkflowMasterId=@WorkflowId AND ParameterCode='ProductPortfolioScore';
INSERT INTO ScoreDefinition(ParameterDefinitionId,Score,Definition,MinValue,MaxValue,MinInclusive,MaxInclusive,ThresholdValue,Unit,ColNo) VALUES
(@P,1,'Very diversified portfolio across all key subsegments',5,NULL,1,1,5,'Subsegments',1),(@P,2,'Broad portfolio with strong presence across most subsegments',4,5,1,0,4,'Subsegments',2),(@P,3,'Moderately diversified portfolio',3,4,1,0,3,'Subsegments',3),(@P,4,'Limited portfolio concentration',2,3,1,0,2,'Subsegments',4),(@P,5,'Highly concentrated portfolio',NULL,2,1,0,0,'Subsegments',5);

SELECT @P=Id FROM ParameterDefinition WHERE WorkflowMasterId=@WorkflowId AND ParameterCode='GeographicDiversificationScore';
INSERT INTO ScoreDefinition(ParameterDefinitionId,Score,Definition,MinValue,MaxValue,MinInclusive,MaxInclusive,ThresholdValue,Unit,ColNo) VALUES
(@P,1,'Highly diversified across regions and exports',4,NULL,1,1,4,'Regions',1),(@P,2,'Healthy diversification across 3-4 regions',3,4,1,0,3,'Regions',2),(@P,3,'Moderate diversification across at least 2 regions',2,3,1,0,2,'Regions',3),(@P,4,'Limited diversification across 1-2 regions',1,2,1,0,1,'Regions',4),(@P,5,'Low diversification / specific region or domestic only',NULL,1,1,0,0,'Regions',5);

SELECT @P=Id FROM ParameterDefinition WHERE WorkflowMasterId=@WorkflowId AND ParameterCode='TechnologyScore';
INSERT INTO ScoreDefinition(ParameterDefinitionId,Score,Definition,MinValue,MaxValue,MinInclusive,MaxInclusive,ThresholdValue,Unit,ColNo) VALUES
(@P,1,'R&D spend > 5% with strong capabilities',5,NULL,0,1,5,'% R&D',1),(@P,2,'R&D spend 2-5% with adequate capabilities',4,5,1,1,4,'% R&D',2),(@P,3,'R&D spend 2-5% with moderate capabilities',3,4,1,0,3,'% R&D',3),(@P,4,'R&D spend 2-5% with limited capabilities',2,3,1,0,2,'% R&D',4),(@P,5,'R&D spend < 2%',NULL,2,1,0,2,'% R&D',5);

DECLARE @json1 NVARCHAR(MAX)=N'[{"WorkflowName":"CorporateBusiness","Weights":{"MarketPositionScore":15,"ScaleScore":15,"ProductPortfolioScore":25,"GeographicDiversificationScore":25,"TechnologyScore":20},"Rules":[{"RuleName":"CalculateWeightedAverage","SuccessEvent":"Calculated","Expression":"true","RuleExpressionType":"LambdaExpression","Actions":{"OnSuccess":{"Name":"OutputExpression","Context":{"Expression":"(input1.MarketPositionScore * 15 + input1.ScaleScore * 15 + input1.ProductPortfolioScore * 25 + input1.GeographicDiversificationScore * 25 + input1.TechnologyScore * 20) / 100.0"}}}}]}]';
DECLARE @json2 NVARCHAR(MAX)=N'[{"WorkflowName":"CorporateBusiness","Weights":{"MarketPositionScore":20,"ScaleScore":10,"ProductPortfolioScore":25,"GeographicDiversificationScore":25,"TechnologyScore":20},"Rules":[{"RuleName":"CalculateWeightedAverage","SuccessEvent":"Calculated","Expression":"true","RuleExpressionType":"LambdaExpression","Actions":{"OnSuccess":{"Name":"OutputExpression","Context":{"Expression":"(input1.MarketPositionScore * 20 + input1.ScaleScore * 10 + input1.ProductPortfolioScore * 25 + input1.GeographicDiversificationScore * 25 + input1.TechnologyScore * 20) / 100.0"}}}}]}]';
INSERT INTO WorkflowJsonVersion(WorkflowMasterId,VersionNo,VersionName,Status,JsonContent,CreatedBy,IsPublished) VALUES(@WorkflowId,1,'09-Sep-2026 - Draft','Published',@json1,'Admin',1),(@WorkflowId,2,'03-Sep-2026','Draft',@json2,'Admin',0);

DECLARE @EntityId INT=(SELECT Id FROM CustomerMaster WHERE EntityName='20 Microns Limited');
DECLARE @IndustryId INT=(SELECT Id FROM IndustryDetails WHERE WorkflowMasterId=@WorkflowId AND IndustryName='Automobiles - Commercial Vehicles');

INSERT INTO FrameworkInstance(WorkflowMasterId,EntityId,EntityName,IndustryName,FrameworkName,Status,WorkflowVersionNo,FrameworkDate,ConsolidatedView)
VALUES(@WorkflowId,@EntityId,'20 Microns Limited','Automobiles - Commercial Vehicles','09-Sep-2026 - Draft','Draft',1,'2026-09-09',0);
DECLARE @FrameworkId INT=SCOPE_IDENTITY();
INSERT INTO BusinessParameterDetails(FrameworkInstanceId,IndustrySegmentId,Weight,Status) VALUES(@FrameworkId,@IndustryId,100,'Saved');
INSERT INTO FrameworkParameterValue(FrameworkInstanceId,ParameterDefinitionId,Value,Score)
SELECT @FrameworkId,Id,CASE ParameterCode WHEN 'MarketPositionScore' THEN 52 WHEN 'ScaleScore' THEN 52 ELSE NULL END,CASE ParameterCode WHEN 'MarketPositionScore' THEN 1 WHEN 'ScaleScore' THEN 5 ELSE 1 END FROM ParameterDefinition WHERE WorkflowMasterId=@WorkflowId;

-- Additional existing frameworks used by the Create/View Framework dropdown.
INSERT INTO FrameworkInstance(WorkflowMasterId,EntityId,EntityName,IndustryName,FrameworkName,Status,WorkflowVersionNo,FrameworkDate,ConsolidatedView)
VALUES
(@WorkflowId,@EntityId,'20 Microns Limited','Automobiles - Commercial Vehicles','03-Sep-2026','Saved',1,'2026-09-03',0),
(@WorkflowId,@EntityId,'20 Microns Limited','Automobiles - Commercial Vehicles','25-Aug-2026','Saved',1,'2026-08-25',0),
(@WorkflowId,@EntityId,'20 Microns Limited','Automobiles - Commercial Vehicles','24-Aug-2026 17:48','Saved',1,'2026-08-24',0),
(@WorkflowId,@EntityId,'20 Microns Limited','Automobiles - Commercial Vehicles','24-Aug-2026 16:09','Saved',1,'2026-08-24',0),
(@WorkflowId,@EntityId,'20 Microns Limited','Automobiles - Commercial Vehicles','19-Aug-2026 16:31','Saved',1,'2026-08-19',0),
(@WorkflowId,@EntityId,'20 Microns Limited','Automobiles - Commercial Vehicles','19-Aug-2026 11:34','Saved',1,'2026-08-19',0),
(@WorkflowId,@EntityId,'20 Microns Limited','Automobiles - Commercial Vehicles','06-Aug-2026','Saved',1,'2026-08-06',0),
(@WorkflowId,@EntityId,'20 Microns Limited','Automobiles - Commercial Vehicles','05-Aug-2026 15:45','Saved',1,'2026-08-05',0);

INSERT INTO BusinessParameterDetails(FrameworkInstanceId,IndustrySegmentId,Weight,Status)
SELECT Id,@IndustryId,100,'Saved' FROM FrameworkInstance WHERE EntityId=@EntityId AND Id>@FrameworkId;
INSERT INTO FrameworkParameterValue(FrameworkInstanceId,ParameterDefinitionId,Value,Score)
SELECT f.Id,p.Id,NULL,1 FROM FrameworkInstance f CROSS JOIN ParameterDefinition p WHERE f.EntityId=@EntityId AND f.Id>@FrameworkId AND p.WorkflowMasterId=@WorkflowId;
GO
