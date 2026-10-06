IF DB_ID(N'CRISP_2_0') IS NULL
    CREATE DATABASE CRISP_2_0;
GO
USE CRISP_2_0;
GO

DROP TABLE IF EXISTS FrameworkParameterValue;
DROP TABLE IF EXISTS BusinessParameterDetails;
DROP TABLE IF EXISTS FrameworkInstance;
DROP TABLE IF EXISTS ScoreDefinition;
DROP TABLE IF EXISTS RuleDefinition;
DROP TABLE IF EXISTS ParameterDefinition;
DROP TABLE IF EXISTS WorkflowJsonVersion;
DROP TABLE IF EXISTS IndustryDetails;
DROP TABLE IF EXISTS WorkflowStep;
DROP TABLE IF EXISTS WorkflowMaster;
DROP TABLE IF EXISTS CustomerMaster;
GO

CREATE TABLE CustomerMaster(
    Id int IDENTITY(1,1) PRIMARY KEY,
    EntityName nvarchar(250) NOT NULL UNIQUE,
    IsActive bit NOT NULL DEFAULT 1
);

CREATE TABLE WorkflowMaster(
    Id int IDENTITY(1,1) PRIMARY KEY,
    WorkflowCode nvarchar(100) NOT NULL UNIQUE,
    WorkflowName nvarchar(200) NOT NULL,
    IsActive bit NOT NULL DEFAULT 1
);

CREATE TABLE WorkflowStep(
    Id int IDENTITY(1,1) PRIMARY KEY,
    WorkflowMasterId int NOT NULL,
    StepName nvarchar(150) NOT NULL,
    DisplayOrder int NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CONSTRAINT FK_WorkflowStep_Workflow FOREIGN KEY(WorkflowMasterId) REFERENCES WorkflowMaster(Id)
);

CREATE TABLE IndustryDetails(
    Id int IDENTITY(1,1) PRIMARY KEY,
    WorkflowMasterId int NOT NULL,
    IndustryName nvarchar(250) NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CONSTRAINT FK_IndustryDetails_Workflow FOREIGN KEY(WorkflowMasterId) REFERENCES WorkflowMaster(Id)
);

CREATE TABLE WorkflowJsonVersion(
    Id int IDENTITY(1,1) PRIMARY KEY,
    WorkflowMasterId int NOT NULL,
    VersionNo int NOT NULL,
    VersionName nvarchar(200) NOT NULL,
    Status nvarchar(30) NOT NULL DEFAULT 'Draft',
    JsonContent nvarchar(max) NOT NULL,
    CreatedOn datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy nvarchar(100) NOT NULL DEFAULT 'Admin',
    IsPublished bit NOT NULL DEFAULT 0,
    CONSTRAINT FK_WorkflowJsonVersion_Workflow FOREIGN KEY(WorkflowMasterId) REFERENCES WorkflowMaster(Id),
    CONSTRAINT UQ_WorkflowJsonVersion UNIQUE(WorkflowMasterId, VersionNo)
);

CREATE TABLE ParameterDefinition(
    Id int IDENTITY(1,1) PRIMARY KEY,
    WorkflowMasterId int NOT NULL,
    ParameterCode nvarchar(100) NOT NULL,
    ParameterName nvarchar(250) NOT NULL,
    WhyItMatters nvarchar(max) NOT NULL DEFAULT '',
    HowDoWeMeasure nvarchar(max) NOT NULL DEFAULT '',
    Weight decimal(10,2) NOT NULL,
    DisplayOrder int NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    IsValueDriven bit NOT NULL DEFAULT 0,
    CONSTRAINT FK_ParameterDefinition_Workflow FOREIGN KEY(WorkflowMasterId) REFERENCES WorkflowMaster(Id),
    CONSTRAINT UQ_ParameterDefinition UNIQUE(WorkflowMasterId, ParameterCode)
);

CREATE TABLE ScoreDefinition(
    Id int IDENTITY(1,1) PRIMARY KEY,
    ParameterDefinitionId int NOT NULL,
    Score int NOT NULL,
    Definition nvarchar(max) NOT NULL,
    ThresholdValue decimal(18,4) NULL,
    MinValue decimal(18,4) NULL,
    MaxValue decimal(18,4) NULL,
    MinInclusive bit NOT NULL DEFAULT 1,
    MaxInclusive bit NOT NULL DEFAULT 1,
    Unit nvarchar(50) NULL,
    RowNo int NOT NULL DEFAULT 0,
    ColNo int NOT NULL DEFAULT 0,
    CONSTRAINT FK_ScoreDefinition_Parameter FOREIGN KEY(ParameterDefinitionId) REFERENCES ParameterDefinition(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_ScoreDefinition UNIQUE(ParameterDefinitionId, Score)
);

CREATE TABLE RuleDefinition(
    Id int IDENTITY(1,1) PRIMARY KEY,
    WorkflowMasterId int NOT NULL,
    RuleName nvarchar(200) NOT NULL,
    SuccessEvent nvarchar(200) NOT NULL DEFAULT 'Calculated',
    Expression nvarchar(max) NOT NULL,
    RuleExpressionType nvarchar(100) NOT NULL DEFAULT 'LambdaExpression',
    OutputExpression nvarchar(max) NOT NULL,
    DisplayOrder int NOT NULL DEFAULT 1,
    IsActive bit NOT NULL DEFAULT 1,
    CONSTRAINT FK_RuleDefinition_Workflow FOREIGN KEY(WorkflowMasterId) REFERENCES WorkflowMaster(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_RuleDefinition UNIQUE(WorkflowMasterId, RuleName)
);

CREATE TABLE FrameworkInstance(
    Id int IDENTITY(1,1) PRIMARY KEY,
    WorkflowMasterId int NOT NULL,
    EntityId int NOT NULL,
    EntityName nvarchar(250) NOT NULL,
    IndustryName nvarchar(250) NOT NULL DEFAULT '',
    FrameworkName nvarchar(250) NOT NULL,
    Status nvarchar(30) NOT NULL DEFAULT 'Draft',
    WorkflowVersionNo int NOT NULL,
    FrameworkDate datetime2 NOT NULL,
    ConsolidatedView bit NOT NULL DEFAULT 0,
    CONSTRAINT FK_FrameworkInstance_Workflow FOREIGN KEY(WorkflowMasterId) REFERENCES WorkflowMaster(Id),
    CONSTRAINT FK_FrameworkInstance_Entity FOREIGN KEY(EntityId) REFERENCES CustomerMaster(Id)
);

CREATE TABLE BusinessParameterDetails(
    Id int IDENTITY(1,1) PRIMARY KEY,
    FrameworkInstanceId int NOT NULL,
    IndustrySegmentId int NOT NULL,
    Weight decimal(10,2) NOT NULL,
    Status nvarchar(30) NOT NULL DEFAULT 'Saved',
    CONSTRAINT FK_BusinessParameterDetails_Framework FOREIGN KEY(FrameworkInstanceId) REFERENCES FrameworkInstance(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BusinessParameterDetails_Industry FOREIGN KEY(IndustrySegmentId) REFERENCES IndustryDetails(Id)
);

CREATE TABLE FrameworkParameterValue(
    Id int IDENTITY(1,1) PRIMARY KEY,
    FrameworkInstanceId int NOT NULL,
    ParameterDefinitionId int NOT NULL,
    Value decimal(18,4) NULL,
    Score int NOT NULL DEFAULT 0,
    CONSTRAINT FK_FrameworkParameterValue_Framework FOREIGN KEY(FrameworkInstanceId) REFERENCES FrameworkInstance(Id) ON DELETE CASCADE,
    CONSTRAINT FK_FrameworkParameterValue_Parameter FOREIGN KEY(ParameterDefinitionId) REFERENCES ParameterDefinition(Id)
);
GO

INSERT CustomerMaster(EntityName, IsActive) VALUES
(N'20 Microns Limited',1),
(N'20 Microns Minerals, Foods Limited-Test 10',1),
(N'20 Microns Nano Minerals Limited',1);

INSERT WorkflowMaster(WorkflowCode, WorkflowName, IsActive)
VALUES(N'CorporateBusiness',N'Corporate Business Framework',1);

DECLARE @W int = SCOPE_IDENTITY();
INSERT WorkflowStep(WorkflowMasterId,StepName,DisplayOrder,IsActive) VALUES
(@W,N'Business',1,1),(@W,N'General',2,1),(@W,N'Others',3,1),(@W,N'Parent/Group',4,1),
(@W,N'Output',5,1),(@W,N'Sensitivity',6,1),(@W,N'Summary',7,1),(@W,N'Benchmarks',8,1);

INSERT IndustryDetails(WorkflowMasterId,IndustryName,IsActive) VALUES
(@W,N'Automobiles - Commercial Vehicles',1),
(@W,N'Automobiles - Passenger Vehicles',1),
(@W,N'Engineering',1);

DECLARE @Json nvarchar(max) = N'[
  {
    "WorkflowName": "CorporateBusiness",
    "GlobalParams": [{ "Name": "Version", "Expression": "1" }],
    "Steps": [
      { "StepName": "Business", "DisplayOrder": 1 },
      { "StepName": "General", "DisplayOrder": 2 },
      { "StepName": "Others", "DisplayOrder": 3 },
      { "StepName": "Parent/Group", "DisplayOrder": 4 },
      { "StepName": "Output", "DisplayOrder": 5 },
      { "StepName": "Sensitivity", "DisplayOrder": 6 },
      { "StepName": "Summary", "DisplayOrder": 7 },
      { "StepName": "Benchmarks", "DisplayOrder": 8 }
    ],
    "Parameters": [
      { "ParameterCode":"MarketPositionScore", "ParameterName":"Market Position & Brand Strength", "WhyItMatters":"Market share is a good proxy to a company''s scale & market position and R&D strengths", "HowDoWeMeasure":"Weighted average market share across key CV segments.", "Weight":15, "DisplayOrder":1, "IsValueDriven":true, "Scores":[
        {"Score":1,"Definition":"Market share greater than 30%","MinValue":30,"MaxValue":null,"MinInclusive":false,"MaxInclusive":true},
        {"Score":2,"Definition":"Market share between 20% and 30%","MinValue":20,"MaxValue":30,"MinInclusive":true,"MaxInclusive":true},
        {"Score":3,"Definition":"Market share between 10% and 20%","MinValue":10,"MaxValue":20,"MinInclusive":true,"MaxInclusive":false},
        {"Score":4,"Definition":"Market share between 5% and 10%","MinValue":5,"MaxValue":10,"MinInclusive":true,"MaxInclusive":false},
        {"Score":5,"Definition":"Market share below 5%","MinValue":null,"MaxValue":5,"MinInclusive":true,"MaxInclusive":false}
      ]},
      { "ParameterCode":"ScaleScore", "ParameterName":"Scale", "WhyItMatters":"Scale of operations, revenues and production capacity are important indicators of business position.", "HowDoWeMeasure":"Revenues from CV operations (Rs. Crore).", "Weight":15, "DisplayOrder":2, "IsValueDriven":true, "Scores":[
        {"Score":1,"Definition":">20,000","MinValue":20000,"MaxValue":null,"MinInclusive":false,"MaxInclusive":true},
        {"Score":2,"Definition":"10,000-20,000","MinValue":10000,"MaxValue":20000,"MinInclusive":true,"MaxInclusive":true},
        {"Score":3,"Definition":"5,000-10,000","MinValue":5000,"MaxValue":10000,"MinInclusive":true,"MaxInclusive":false},
        {"Score":4,"Definition":"1,000-5,000","MinValue":1000,"MaxValue":5000,"MinInclusive":true,"MaxInclusive":false},
        {"Score":5,"Definition":"<1,000","MinValue":null,"MaxValue":1000,"MinInclusive":true,"MaxInclusive":false}
      ]},
      { "ParameterCode":"ProductPortfolioScore", "ParameterName":"Product Portfolio", "WhyItMatters":"A strong product portfolio supports a diverse customer profile and reduces concentration risk.", "HowDoWeMeasure":"We evaluate product portfolio across CV subsegments.", "Weight":25, "DisplayOrder":3, "IsValueDriven":false, "Scores":[
        {"Score":1,"Definition":"Strong product/segment diversity across all 5 sub segments"},
        {"Score":2,"Definition":"Strong product/segment diversity across 3-4 sub segments"},
        {"Score":3,"Definition":"Strong product/segment diversity across 2-3 sub segments"},
        {"Score":4,"Definition":"High concentration on a single product/segment 80-90%"},
        {"Score":5,"Definition":"High concentration on a single product/segment more than 90%"}
      ]},
      { "ParameterCode":"GeographicDiversificationScore", "ParameterName":"Geographic Diversification", "WhyItMatters":"Geographic diversification helps mitigate risks arising from demand slowdown in a particular geography.", "HowDoWeMeasure":"Domestic regional and export diversification is considered.", "Weight":25, "DisplayOrder":4, "IsValueDriven":false, "Scores":[
        {"Score":1,"Definition":"Highly diversified with healthy revenue/volume mix across majority of domestic regions or high export diversification"},
        {"Score":2,"Definition":"Healthy diversification with healthy revenue/volume mix across 3-4 regions or healthy export diversification"},
        {"Score":3,"Definition":"Moderately diversified with healthy revenue/volume mix across at least 2 regions or moderate export diversification"},
        {"Score":4,"Definition":"Relatively low diversification with healthy revenue/volume mix across only 1-2 regions or limited export diversification"},
        {"Score":5,"Definition":"Low diversification with revenues/volume limited to a specific region; largely domestic market"}
      ]},
      { "ParameterCode":"TechnologyScore", "ParameterName":"Technology & Product Development Capabilities", "WhyItMatters":"OEMs need to continuously refresh offerings and meet regulatory changes.", "HowDoWeMeasure":"Product design/engineering capabilities and new product development plans.", "Weight":20, "DisplayOrder":5, "IsValueDriven":false, "Scores":[
        {"Score":1,"Definition":"R&D spend is above 5% of revenues; strong track record and technology support"},
        {"Score":2,"Definition":"R&D spend is between 2 to 5% of revenues; adequate technological capabilities"},
        {"Score":3,"Definition":"R&D spend is between 2 to 5% with limited technological support"},
        {"Score":4,"Definition":"R&D spend is between 2 to 5% with limited support"},
        {"Score":5,"Definition":"R&D spend is lower than 2% of revenues; product development capability remains limited"}
      ]}
    ],
    "Rules": [
      {"RuleName":"CalculateWeightedAverage","SuccessEvent":"Calculated","Expression":"true","RuleExpressionType":"LambdaExpression","OutputExpression":"(input1.MarketPositionScore * 15 + input1.ScaleScore * 15 + input1.ProductPortfolioScore * 25 + input1.GeographicDiversificationScore * 25 + input1.TechnologyScore * 20) / 100.0"}
    ]
  }
]';

INSERT WorkflowJsonVersion(WorkflowMasterId,VersionNo,VersionName,Status,JsonContent,CreatedBy,IsPublished)
VALUES(@W,1,N'09-Sep-2026 - Draft',N'Published',@Json,N'Admin',1);

INSERT ParameterDefinition(WorkflowMasterId,ParameterCode,ParameterName,WhyItMatters,HowDoWeMeasure,Weight,DisplayOrder,IsActive,IsValueDriven)
SELECT @W, v.ParameterCode,v.ParameterName,v.WhyItMatters,v.HowDoWeMeasure,v.Weight,v.DisplayOrder,1,v.IsValueDriven
FROM (VALUES
('MarketPositionScore','Market Position & Brand Strength','Market share is a good proxy to a company''s scale & market position and R&D strengths','Weighted average market share across key CV segments.',15,1,1),
('ScaleScore','Scale','Scale of operations, revenues and production capacity are important indicators of business position.','Revenues from CV operations (Rs. Crore).',15,2,1),
('ProductPortfolioScore','Product Portfolio','A strong product portfolio supports a diverse customer profile and reduces concentration risk.','We evaluate product portfolio across CV subsegments.',25,3,0),
('GeographicDiversificationScore','Geographic Diversification','Geographic diversification helps mitigate risks arising from demand slowdown in a particular geography.','Domestic regional and export diversification is considered.',25,4,0),
('TechnologyScore','Technology & Product Development Capabilities','OEMs need to continuously refresh offerings and meet regulatory changes.','Product design/engineering capabilities and new product development plans.',20,5,0)
) v(ParameterCode,ParameterName,WhyItMatters,HowDoWeMeasure,Weight,DisplayOrder,IsValueDriven);

DECLARE @P1 int=(SELECT Id FROM ParameterDefinition WHERE WorkflowMasterId=@W AND ParameterCode='MarketPositionScore');
DECLARE @P2 int=(SELECT Id FROM ParameterDefinition WHERE WorkflowMasterId=@W AND ParameterCode='ScaleScore');
DECLARE @P3 int=(SELECT Id FROM ParameterDefinition WHERE WorkflowMasterId=@W AND ParameterCode='ProductPortfolioScore');
DECLARE @P4 int=(SELECT Id FROM ParameterDefinition WHERE WorkflowMasterId=@W AND ParameterCode='GeographicDiversificationScore');
DECLARE @P5 int=(SELECT Id FROM ParameterDefinition WHERE WorkflowMasterId=@W AND ParameterCode='TechnologyScore');

INSERT ScoreDefinition(ParameterDefinitionId,Score,Definition,MinValue,MaxValue,MinInclusive,MaxInclusive) VALUES
(@P1,1,N'Market share greater than 30%',30,NULL,0,1),(@P1,2,N'Market share between 20% and 30%',20,30,1,1),(@P1,3,N'Market share between 10% and 20%',10,20,1,0),(@P1,4,N'Market share between 5% and 10%',5,10,1,0),(@P1,5,N'Market share below 5%',NULL,5,1,0),
(@P2,1,N'>20,000',20000,NULL,0,1),(@P2,2,N'10,000-20,000',10000,20000,1,1),(@P2,3,N'5,000-10,000',5000,10000,1,0),(@P2,4,N'1,000-5,000',1000,5000,1,0),(@P2,5,N'<1,000',NULL,1000,1,0),
(@P3,1,N'Strong product/segment diversity across all 5 sub segments',NULL,NULL,1,1),(@P3,2,N'Strong product/segment diversity across 3-4 sub segments',NULL,NULL,1,1),(@P3,3,N'Strong product/segment diversity across 2-3 sub segments',NULL,NULL,1,1),(@P3,4,N'High concentration on a single product/segment 80-90%',NULL,NULL,1,1),(@P3,5,N'High concentration on a single product/segment more than 90%',NULL,NULL,1,1),
(@P4,1,N'Highly diversified with healthy revenue/volume mix across majority of domestic regions or high export diversification',NULL,NULL,1,1),(@P4,2,N'Healthy diversification with healthy revenue/volume mix across 3-4 regions or healthy export diversification',NULL,NULL,1,1),(@P4,3,N'Moderately diversified with healthy revenue/volume mix across at least 2 regions or moderate export diversification',NULL,NULL,1,1),(@P4,4,N'Relatively low diversification with healthy revenue/volume mix across only 1-2 regions or limited export diversification',NULL,NULL,1,1),(@P4,5,N'Low diversification with revenues/volume limited to a specific region; largely domestic market',NULL,NULL,1,1),
(@P5,1,N'R&D spend is above 5% of revenues; strong track record and technology support',NULL,NULL,1,1),(@P5,2,N'R&D spend is between 2 to 5% of revenues; adequate technological capabilities',NULL,NULL,1,1),(@P5,3,N'R&D spend is between 2 to 5% with limited technological support',NULL,NULL,1,1),(@P5,4,N'R&D spend is between 2 to 5% with limited support',NULL,NULL,1,1),(@P5,5,N'R&D spend is lower than 2% of revenues; product development capability remains limited',NULL,NULL,1,1);

INSERT RuleDefinition(WorkflowMasterId, RuleName, SuccessEvent, Expression, RuleExpressionType, OutputExpression, DisplayOrder, IsActive)
VALUES(@W, N'CalculateWeightedAverage', N'Calculated', N'true', N'LambdaExpression', N'(input1.MarketPositionScore * 15 + input1.ScaleScore * 15 + input1.ProductPortfolioScore * 25 + input1.GeographicDiversificationScore * 25 + input1.TechnologyScore * 20) / 100.0', 1, 1);

DECLARE @E int=(SELECT Id FROM CustomerMaster WHERE EntityName=N'20 Microns Limited');
DECLARE @I int=(SELECT Id FROM IndustryDetails WHERE WorkflowMasterId=@W AND IndustryName=N'Automobiles - Commercial Vehicles');
INSERT FrameworkInstance(WorkflowMasterId,EntityId,EntityName,IndustryName,FrameworkName,Status,WorkflowVersionNo,FrameworkDate,ConsolidatedView)
VALUES(@W,@E,N'20 Microns Limited',N'Automobiles - Commercial Vehicles',N'09-Sep-2026 - Draft',N'Draft',1,'2026-09-09',0);
DECLARE @F int=SCOPE_IDENTITY();
INSERT BusinessParameterDetails(FrameworkInstanceId,IndustrySegmentId,Weight,Status) VALUES(@F,@I,100,N'Saved');

INSERT FrameworkParameterValue(FrameworkInstanceId,ParameterDefinitionId,Value,Score)
VALUES
(@F,@P1,52,1),
(@F,@P2,52,5),
(@F,@P3,NULL,1),
(@F,@P4,NULL,1),
(@F,@P5,NULL,1);

INSERT FrameworkInstance(WorkflowMasterId,EntityId,EntityName,IndustryName,FrameworkName,Status,WorkflowVersionNo,FrameworkDate,ConsolidatedView)
VALUES(@W,@E,N'20 Microns Limited',N'Automobiles - Commercial Vehicles',N'03-Sep-2026',N'Saved',1,'2026-09-03',0),
(@W,@E,N'20 Microns Limited',N'Automobiles - Commercial Vehicles',N'25-Aug-2026',N'Saved',1,'2026-08-25',0),
(@W,@E,N'20 Microns Limited',N'Automobiles - Commercial Vehicles',N'24-Aug-2026 17:48',N'Saved',1,'2026-08-24T17:48:00',0),
(@W,@E,N'20 Microns Limited',N'Automobiles - Commercial Vehicles',N'24-Aug-2026 16:09',N'Saved',1,'2026-08-24T16:09:00',0),
(@W,@E,N'20 Microns Limited',N'Automobiles - Commercial Vehicles',N'19-Aug-2026 16:31',N'Saved',1,'2026-08-19T16:31:00',0),
(@W,@E,N'20 Microns Limited',N'Automobiles - Commercial Vehicles',N'19-Aug-2026 11:34',N'Saved',1,'2026-08-19T11:34:00',0),
(@W,@E,N'20 Microns Limited',N'Automobiles - Commercial Vehicles',N'06-Aug-2026',N'Saved',1,'2026-08-06',0),
(@W,@E,N'20 Microns Limited',N'Automobiles - Commercial Vehicles',N'05-Aug-2026 15:45',N'Saved',1,'2026-08-05T15:45:00',0);
GO
