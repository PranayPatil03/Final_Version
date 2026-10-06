USE CRISPDemo;
GO

IF COL_LENGTH('ParameterDefinition', 'IsValueDriven') IS NULL
    ALTER TABLE ParameterDefinition ADD IsValueDriven BIT NOT NULL CONSTRAINT DF_ParameterDefinition_IsValueDriven DEFAULT 0;
GO

IF COL_LENGTH('ScoreDefinition', 'MinValue') IS NULL
    ALTER TABLE ScoreDefinition ADD MinValue DECIMAL(18,4) NULL;
IF COL_LENGTH('ScoreDefinition', 'MaxValue') IS NULL
    ALTER TABLE ScoreDefinition ADD MaxValue DECIMAL(18,4) NULL;
IF COL_LENGTH('ScoreDefinition', 'MinInclusive') IS NULL
    ALTER TABLE ScoreDefinition ADD MinInclusive BIT NOT NULL CONSTRAINT DF_ScoreDefinition_MinInclusive DEFAULT 1;
IF COL_LENGTH('ScoreDefinition', 'MaxInclusive') IS NULL
    ALTER TABLE ScoreDefinition ADD MaxInclusive BIT NOT NULL CONSTRAINT DF_ScoreDefinition_MaxInclusive DEFAULT 1;
GO

UPDATE ParameterDefinition
SET IsValueDriven = 1
WHERE ParameterCode IN ('MarketPositionScore','ScaleScore');

-- Market Position & Brand Strength
UPDATE sd SET MinValue = 30, MaxValue = NULL, MinInclusive = 0, MaxInclusive = 1
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'MarketPositionScore' AND sd.Score = 1;
UPDATE sd SET MinValue = 20, MaxValue = 30, MinInclusive = 1, MaxInclusive = 1
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'MarketPositionScore' AND sd.Score = 2;
UPDATE sd SET MinValue = 10, MaxValue = 20, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'MarketPositionScore' AND sd.Score = 3;
UPDATE sd SET MinValue = 5, MaxValue = 10, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'MarketPositionScore' AND sd.Score = 4;
UPDATE sd SET MinValue = NULL, MaxValue = 5, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'MarketPositionScore' AND sd.Score = 5;

-- Scale
UPDATE sd SET MinValue = 20000, MaxValue = NULL, MinInclusive = 0, MaxInclusive = 1
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'ScaleScore' AND sd.Score = 1;
UPDATE sd SET MinValue = 10000, MaxValue = 20000, MinInclusive = 1, MaxInclusive = 1
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'ScaleScore' AND sd.Score = 2;
UPDATE sd SET MinValue = 5000, MaxValue = 10000, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'ScaleScore' AND sd.Score = 3;
UPDATE sd SET MinValue = 1000, MaxValue = 5000, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'ScaleScore' AND sd.Score = 4;
UPDATE sd SET MinValue = NULL, MaxValue = 1000, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'ScaleScore' AND sd.Score = 5;

-- Product Portfolio: number of qualifying subsegments
UPDATE sd SET MinValue = 5, MaxValue = NULL, MinInclusive = 1, MaxInclusive = 1
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'ProductPortfolioScore' AND sd.Score = 1;
UPDATE sd SET MinValue = 4, MaxValue = 5, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'ProductPortfolioScore' AND sd.Score = 2;
UPDATE sd SET MinValue = 3, MaxValue = 4, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'ProductPortfolioScore' AND sd.Score = 3;
UPDATE sd SET MinValue = 2, MaxValue = 3, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'ProductPortfolioScore' AND sd.Score = 4;
UPDATE sd SET MinValue = NULL, MaxValue = 2, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'ProductPortfolioScore' AND sd.Score = 5;

-- Geographic diversification: number of regions
UPDATE sd SET MinValue = 4, MaxValue = NULL, MinInclusive = 1, MaxInclusive = 1
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'GeographicDiversificationScore' AND sd.Score = 1;
UPDATE sd SET MinValue = 3, MaxValue = 4, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'GeographicDiversificationScore' AND sd.Score = 2;
UPDATE sd SET MinValue = 2, MaxValue = 3, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'GeographicDiversificationScore' AND sd.Score = 3;
UPDATE sd SET MinValue = 1, MaxValue = 2, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'GeographicDiversificationScore' AND sd.Score = 4;
UPDATE sd SET MinValue = NULL, MaxValue = 1, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'GeographicDiversificationScore' AND sd.Score = 5;

-- Technology / R&D spend
UPDATE sd SET MinValue = 5, MaxValue = NULL, MinInclusive = 0, MaxInclusive = 1
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'TechnologyScore' AND sd.Score = 1;
UPDATE sd SET MinValue = 4, MaxValue = 5, MinInclusive = 1, MaxInclusive = 1
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'TechnologyScore' AND sd.Score = 2;
UPDATE sd SET MinValue = 3, MaxValue = 4, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'TechnologyScore' AND sd.Score = 3;
UPDATE sd SET MinValue = 2, MaxValue = 3, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'TechnologyScore' AND sd.Score = 4;
UPDATE sd SET MinValue = NULL, MaxValue = 2, MinInclusive = 1, MaxInclusive = 0
FROM ScoreDefinition sd JOIN ParameterDefinition p ON p.Id = sd.ParameterDefinitionId
WHERE p.ParameterCode = 'TechnologyScore' AND sd.Score = 5;
GO

-- Match the sample screen: Market Position = 52 => score 1, Scale = 52 => score 5.
UPDATE fp
SET Value = CASE p.ParameterCode WHEN 'MarketPositionScore' THEN 52 WHEN 'ScaleScore' THEN 52 ELSE NULL END,
    Score = CASE p.ParameterCode WHEN 'MarketPositionScore' THEN 1 WHEN 'ScaleScore' THEN 5 ELSE 1 END
FROM FrameworkParameterValue fp
JOIN ParameterDefinition p ON p.Id = fp.ParameterDefinitionId
WHERE fp.FrameworkInstanceId = (SELECT TOP 1 Id FROM FrameworkInstance ORDER BY Id)
  AND p.ParameterCode IN ('MarketPositionScore','ScaleScore','ProductPortfolioScore','GeographicDiversificationScore','TechnologyScore');
GO
