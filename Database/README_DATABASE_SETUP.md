# CRISP database setup

## Existing `CRISPDemo` database

Your current database schema is already compatible with this build. The lookup data shown by the verification query is:

- `CustomerMaster` → Entity Name search
- `FrameworkInstance` → Create/View Framework dropdown
- `WorkflowMaster` → active framework/workflow
- `WorkflowJsonVersion` → JSON versions and published version
- `IndustryDetails` → industry segment lookup
- `BusinessParameterDetails` → selected industry/business-parameter details

Run `04_Verify_CRISP_Lookups.sql` or `05_Verify_Existing_CRISPDemo.sql` only to inspect the data. No schema change is required for `RouteKey` or `ActiveVersionNo`.

## New database

Run:

1. `01_Create_DynamicRuleEngine_Framework.sql`
2. `02_Add_Value_Dependent_Scoring.sql` if additional value-dependent scoring data is needed.

The fresh database script uses the same schema expected by the EF Core model and does **not** create `WorkflowMaster.RouteKey` or `WorkflowMaster.ActiveVersionNo`.

## Important

Do not run obsolete upgrade scripts that add `RouteKey`/`ActiveVersionNo` to the current schema. They are retained only as historical reference.
