/* Run only when upgrading an existing CRISP_2_0 / CRISPDemo database.
   This script does not delete existing data. */

IF OBJECT_ID(N'dbo.RuleDefinition', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleDefinition
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RuleDefinition PRIMARY KEY,
        WorkflowMasterId int NOT NULL,
        RuleName nvarchar(200) NOT NULL,
        SuccessEvent nvarchar(200) NOT NULL CONSTRAINT DF_RuleDefinition_SuccessEvent DEFAULT N'Calculated',
        Expression nvarchar(max) NOT NULL,
        RuleExpressionType nvarchar(100) NOT NULL CONSTRAINT DF_RuleDefinition_Type DEFAULT N'LambdaExpression',
        OutputExpression nvarchar(max) NOT NULL,
        DisplayOrder int NOT NULL CONSTRAINT DF_RuleDefinition_DisplayOrder DEFAULT 1,
        IsActive bit NOT NULL CONSTRAINT DF_RuleDefinition_IsActive DEFAULT 1,
        CONSTRAINT FK_RuleDefinition_Workflow FOREIGN KEY (WorkflowMasterId)
            REFERENCES dbo.WorkflowMaster(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_RuleDefinition UNIQUE (WorkflowMasterId, RuleName)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.RuleDefinition r
    INNER JOIN dbo.WorkflowMaster w ON w.Id = r.WorkflowMasterId
    WHERE w.WorkflowCode = N'CorporateBusiness'
      AND r.RuleName = N'CalculateWeightedAverage'
)
BEGIN
    INSERT dbo.RuleDefinition
    (
        WorkflowMasterId,
        RuleName,
        SuccessEvent,
        Expression,
        RuleExpressionType,
        OutputExpression,
        DisplayOrder,
        IsActive
    )
    SELECT
        Id,
        N'CalculateWeightedAverage',
        N'Calculated',
        N'true',
        N'LambdaExpression',
        N'(input1.MarketPositionScore * 15 + input1.ScaleScore * 15 + input1.ProductPortfolioScore * 25 + input1.GeographicDiversificationScore * 25 + input1.TechnologyScore * 20) / 100.0',
        1,
        1
    FROM dbo.WorkflowMaster
    WHERE WorkflowCode = N'CorporateBusiness';
END;
GO
