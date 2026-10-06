# CRISP_2.0

ICRA-style Credit Rating Framework demo built with .NET 10, Blazor Server, MudBlazor, Clean Architecture, EF Core + SQL Server LocalDB and a JSON-driven Rules Engine.

## Architecture
UI (CRISP.Web) -> API (CRISP.Api) -> Application (Services/DTOs) -> Infrastructure (Repositories/EF Core) -> SQL Server

Rules Engine is a separate class library under Libraries/RulesEngine and is called by the Application evaluation flow.

## Data-driven behaviour
- CustomerMaster drives entity lookup.
- FrameworkInstance.EntityId drives the Create/View Framework list.
- Published/latest workflow JSON is the source definition for parameters, score definitions and calculation rules.
- Admin Parameters and Rules pages modify the JSON definition and save a new version.
- Saving a JSON version synchronizes ParameterDefinition/ScoreDefinition tables so existing framework value rows remain usable.
- Framework screen reads the synchronized definition; no parameter names, weights or scoring rules are hardcoded in Razor.

## Database
Use Database/01_Create_CRISP_2.0.sql against `(localdb)\MSSQLLocalDB`. It creates CRISP_2_0 and seeds the 20 Microns example and CorporateBusiness JSON.

## Run
API: http://localhost:5101/swagger
Web: http://localhost:4200/crf/corporate

The solution intentionally uses no EF migrations.

## Final UI folder structure

CRISP.Web/Pages is organized by feature instead of keeping all Razor pages in one folder:

- `Pages/CRF/Corporate.razor`
- `Pages/WorkflowJson/ManageJson.razor`
- `Pages/WorkflowJson/Parameters.razor`
- `Pages/WorkflowJson/Rules.razor`
- `Pages/WorkflowJson/Evaluate.razor`
- `Pages/Information/FinancialInformation.razor`
- `Pages/Reports/ReportGeneration.razor`
- `Pages/Home.razor`
- `Pages/Error.razor`

Corporate dialogs are under `Components/Corporate/Dialogs`.

## Corporate flow

1. Open Corporate. The eight workflow steps are loaded immediately from the workflow context.
2. Search/select an entity. Frameworks are loaded only for that entity.
3. If the entity has a Draft framework, only the existing frameworks are shown; `Create New Framework` is hidden.
4. If the entity has no Draft, existing saved frameworks plus `Create New Framework` are shown.
5. Selecting a framework loads the framework, industry segments, parameter values and scores in one framework request.
6. `Edit/View Score` opens the Business Score dialog.
7. Run Framework sends the selected published Workflow JSON and inputs through the separate `Libraries/RulesEngine` library.
8. Workflow JSON changes are saved as a new version and synchronized transactionally to ParameterDefinition, ScoreDefinition, RuleDefinition and WorkflowStep.
