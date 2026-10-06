# CRISP Entity / Framework lookup fix

The Create/View Framework lookup is now data-driven from the API and CustomerMaster.

## Entity Name
- CustomerMaster is loaded from `GET /api/framework/entities`.
- The MudAutocomplete filters the API-loaded list immediately.
- If the typed value is not in the initial list, the UI calls the API again with the typed search text.
- `Immediate=true` and `OpenOnFocus=true` make the suggestion list open while typing.
- No entity names are hardcoded in the Razor UI.

## Create/View Framework
- Disabled until an entity is selected.
- After entity selection, the UI calls `GET /api/framework/entities/{entityId}/frameworks?workflowCode=...`.
- `@key="SelectedEntity?.Id"` forces MudSelect to refresh when the selected entity changes.
- The dropdown contains `Create New Framework` plus the existing framework/draft records returned by the API.

## CSS
The MudBlazor autocomplete/popover z-index is raised so the suggestion list is visible above the framework grid.
