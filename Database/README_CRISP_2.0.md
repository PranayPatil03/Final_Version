# CRISP 2.0 database

1. Connect SSMS to `(localdb)\MSSQLLocalDB`.
2. Run `01_Create_CRISP_2.0.sql`.
3. Database created: `CRISP_2_0`.
4. The API connection string in appsettings.json points to `CRISP_2_0`.

JSON is stored in `WorkflowJsonVersion.JsonContent`. ParameterDefinition and ScoreDefinition are a synchronized relational read model for fast framework rendering and value scoring.
