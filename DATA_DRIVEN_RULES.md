# Data-driven rule framework

Runtime business data is not hardcoded in the Web UI.

The Web page receives a FrameworkInstance ID and loads:
- entity name
- industry
- framework name/status/date
- workflow steps
- parameter definitions
- weights
- score definitions
- saved parameter values and scores
- workflow JSON versions

from SQL Server through API -> Application Service -> Repository -> EF Core.

Calculation formulas are stored in WorkflowJsonVersion.JsonContent and executed by the uploaded RulesEngine library.

The SQL script contains seed data only so a new database has demo records. Those INSERT statements are database seed data, not runtime hardcoded application data.
