# IIS Deployment

1. Install .NET 10 ASP.NET Core Hosting Bundle on the server.
2. Publish API: `dotnet publish src/API -c Release -o C:\inetpub\RuleEngine\API`.
3. Publish Web: `dotnet publish src/Web -c Release -o C:\inetpub\RuleEngine\Web`.
4. Create two IIS applications/sites (or two applications under one site): API and Web.
5. API application pool: **No Managed Code**.
6. Web application pool: **No Managed Code**.
7. In Web `appsettings.json`, set `ApiBaseUrl` to the IIS API application URL.
8. Give the IIS app-pool identity permission to the publish directories.
9. Confirm SQL Server connectivity from the IIS server.
