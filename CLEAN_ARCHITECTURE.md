# CRISP 2.0 Clean Architecture

## Presentation
CRISP.Web contains Razor pages, MudBlazor UI and a typed FrameworkApiClient. No EF Core or SQL is referenced by the UI.

## API
CRISP.Api contains thin controllers only. Controllers delegate to Application services.

## Application
DTOs, interfaces and business orchestration. JSON definition validation/version creation happens in the Application service.

## Infrastructure
EF Core DbContext/repositories, SQL Server persistence and the Rules Engine adapter.

## Domain
Persistence/domain entities only.

## Rules
Libraries/RulesEngine is a separate class library. Runtime calculation consumes the JSON version selected by the framework.
