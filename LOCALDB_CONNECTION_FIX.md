# CRISP v5 - Database Connection Fix

The SQL screenshot shows the CRISPDemo database is hosted on:

`(localdb)\\MSSQLLocalDB`

The previous API configuration used `Server=localhost`, which can point to a different SQL Server instance. That is why SSMS showed CustomerMaster and FrameworkInstance data while the API returned no lookup data.

The API `appsettings.json` now uses:

`Server=(localdb)\\MSSQLLocalDB;Database=CRISPDemo;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True`

No UI data is hardcoded.

After replacing the project:
1. Stop the old API/Web processes.
2. Start CRISP.Api first.
3. Confirm the API is on `http://localhost:5101`.
4. Start CRISP.Web on `http://localhost:4200`.
5. Open `/crf/corporate` and type `20 mic`.
