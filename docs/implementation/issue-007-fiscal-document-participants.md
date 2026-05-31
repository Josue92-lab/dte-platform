# Issue #007: Fiscal Document Participants

**Status:** Completed  
**Date:** 2026-05-31  

## Objective
Establish how participant identity information is captured within a `FiscalDocument` by introducing immutable snapshot Value Objects for the issuer and recipient, and embedding them in the aggregate.

## Scope
- Creation of `IssuerSnapshot` Value Object — all fields required per MH `emisor` block.
- Creation of `RecipientSnapshot` Value Object — only `PartyName` required, all other fields nullable per MH `receptor` block.
- Modification of `FiscalDocument.Create()` to accept `IssuerSnapshot` (required) and `RecipientSnapshot?` (optional).
- Extension of `FiscalDocumentCreated` domain event to include participant snapshots.

## Out of Scope
- Master records / CRM entities.
- Catalog validation against MH catalogs.
- DTE-type-specific validation rules (e.g., CCF requires non-null recipient).
- `codEstable` / `codPuntoVenta` on IssuerSnapshot (deferred due to ownership overlap with `ControlNumberSeries`).
- Persistence, EF Core, repositories, API contracts, infrastructure.

## Files Created/Modified
- `src/DTE.Domain/ValueObjects/IssuerSnapshot.cs` [NEW]: Composes NIT, NRC, PartyName, EconomicActivityCode, Address, PhoneNumber, EmailAddress — all required.
- `src/DTE.Domain/ValueObjects/RecipientSnapshot.cs` [NEW]: Composes PartyName (required), with optional DocumentIdentifier, NRC, EconomicActivityCode, Address, PhoneNumber, EmailAddress.
- `src/DTE.Domain/Aggregates/FiscalDocuments/FiscalDocument.cs` [MODIFIED]: Added `Issuer` and `Recipient` properties; extended `Create()` factory with null-issuer validation.
- `src/DTE.Domain/Events/FiscalDocumentCreated.cs` [MODIFIED]: Added `Issuer` and `Recipient` to event record.
- `tests/DTE.Domain.Tests/ValueObjects/IssuerSnapshotTests.cs` [NEW]: 10 tests covering happy path, all null-field rejections, and equality.
- `tests/DTE.Domain.Tests/ValueObjects/RecipientSnapshotTests.cs` [NEW]: 6 tests covering minimal/full construction, null-name rejection, and equality.
- `tests/DTE.Domain.Tests/Aggregates/FiscalDocuments/FiscalDocumentTests.cs` [MODIFIED]: Updated all factory calls; added tests for both-participants case and null-issuer rejection.

## Architectural Decisions Used
- **Snapshot Pattern:** Participant data is frozen at document creation time as immutable Value Objects. No foreign key references to mutable master records.
- **Structural Asymmetry:** `IssuerSnapshot` and `RecipientSnapshot` are separate types reflecting the MH schema's fundamentally different `emisor` (all required) vs `receptor` (mostly nullable) structures.
- **Full Reuse:** Seven existing VOs composed without duplication: `TaxIdentifier`, `PartyName`, `EconomicActivityCode`, `Address`, `PhoneNumber`, `EmailAddress`, `DepartmentCode`/`MunicipalityCode` (via Address).

## Regulatory Sources Used
- [fe-f-v2.json — emisor](file:///c:/Temp/dte-platform/docs/mh/svfe-json-schemas/v2/fe-f-v2.json) Lines 156–274
- [fe-f-v2.json — receptor](file:///c:/Temp/dte-platform/docs/mh/svfe-json-schemas/v2/fe-f-v2.json) Lines 275–399

## Testing Summary
- `IssuerSnapshotTests`: Valid construction, seven null-required-field rejection tests, equality and inequality.
- `RecipientSnapshotTests`: Minimal construction (name only), full construction, null-name rejection, equality with nullable fields.
- `FiscalDocumentTests`: Updated for new factory signature; added issuer-only creation, both-participants creation, null-issuer rejection, event snapshot verification.

## Follow-up Issues
- **#008**: Catalog Domain (V1) — create entities for economic activities, departments, municipalities.
- **#009**: DTE-Type-Specific Validation — enforce per-type rules (e.g., CCF requires non-null recipient with NRC).
- **T-003**: `codEstable` / `codPuntoVenta` on IssuerSnapshot.
- **T-004**: Document Line Items.

## Traceability References
- [Issue #007 Specification](file:///C:/Users/z004vfcd/.gemini/antigravity-ide/brain/4789ed83-1880-4dac-b209-74b7ec834c05/issue-007-fiscal-document-participants.md)
- [Issue #006: Counterparty Identity Model](file:///c:/Temp/dte-platform/docs/implementation/issue-006-counterparty-identity-model.md)
