# CRISP Entity / Framework Lookup Fix v4

The lookup issue was caused by the UI relying only on `CustomerMaster` rows and framework rows being linked strictly by `EntityId`.

The API now uses this data-driven flow:

1. `GET /api/framework/entities?search=20`
2. Primary source: `CustomerMaster`.
3. If no matching CustomerMaster row exists, fallback to distinct `FrameworkInstance.EntityId + EntityName` records from the existing database.
4. After an entity is selected, `GET /api/framework/entities/{entityId}/frameworks?workflowCode=CorporateBusiness`.
5. Framework lookup matches by `EntityId` OR, for upgraded legacy rows, the selected CustomerMaster `EntityName`.

No company name is hardcoded in Razor.

If the database has neither CustomerMaster records nor FrameworkInstance records, the API correctly returns an empty list; data must then be inserted/linked in SQL.

Run `Database/04_Verify_CRISP_Lookups.sql` once against `CRISPDemo` to repair CustomerMaster activation and legacy links.
