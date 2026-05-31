# Issue #006: Counterparty Identity Model

**Status:** Completed  
**Date:** 2026-05-31  

## Objective
Create the minimum set of domain Value Objects required to represent the identity of fiscal document participants (Issuer and Recipient), as defined by the MH `emisor` and `receptor` schema blocks in `fe-f-v2.json`.

## Scope
Creation of five new `ValueObject` types:
- `PartyName` — Legal name (required, 1–250 chars) and optional commercial name (1–150 chars)
- `EconomicActivityCode` — Activity code (5–6 chars) paired with its description (5–150 chars)
- `DepartmentCode` — Opaque 2-character reference to MH catalog CAT-012
- `MunicipalityCode` — Opaque 2-character reference to MH catalog CAT-013
- `Address` — Structured composition of `DepartmentCode`, `MunicipalityCode`, and complement text (1–200 chars)

## Out of Scope
- Counterparty composite type (Issuer/Recipient snapshots) — deferred to Issue #007.
- FiscalDocument modification — aggregate is not touched.
- Catalog Domain (EconomicActivity, Department, Municipality entities) — deferred to Issue #008.
- MH Catalog Import / Synchronization.
- Persistence, Repositories, EF Core mappings.
- API Contracts, Controllers, DTOs.
- Infrastructure concerns.

## Files Created
- `src/DTE.Domain/ValueObjects/PartyName.cs`: Encapsulates `LegalName` and optional `CommercialName`.
- `src/DTE.Domain/ValueObjects/EconomicActivityCode.cs`: Encapsulates `Code` and `Description` per MH `codActividad`/`descActividad`.
- `src/DTE.Domain/ValueObjects/DepartmentCode.cs`: Opaque 2-char code referencing MH department catalog.
- `src/DTE.Domain/ValueObjects/MunicipalityCode.cs`: Opaque 2-char code referencing MH municipality catalog.
- `src/DTE.Domain/ValueObjects/Address.cs`: Composes `DepartmentCode`, `MunicipalityCode`, and `Complement`.
- `tests/DTE.Domain.Tests/ValueObjects/PartyNameTests.cs`: Unit tests for PartyName.
- `tests/DTE.Domain.Tests/ValueObjects/EconomicActivityCodeTests.cs`: Unit tests for EconomicActivityCode.
- `tests/DTE.Domain.Tests/ValueObjects/DepartmentCodeTests.cs`: Unit tests for DepartmentCode.
- `tests/DTE.Domain.Tests/ValueObjects/MunicipalityCodeTests.cs`: Unit tests for MunicipalityCode.
- `tests/DTE.Domain.Tests/ValueObjects/AddressTests.cs`: Unit tests for Address.

## Architectural Decisions Used
- **Factory Methods:** All VOs expose `Result<T> Create(...)` factories; no public constructors.
- **Opaque Codes:** `DepartmentCode` and `MunicipalityCode` enforce structural shape only (2 chars). Semantic validation against MH catalogs is the future Catalog domain's responsibility.
- **Paired Code+Description:** `EconomicActivityCode` carries both `codActividad` and `descActividad` because the MH schema always requires them together in the same payload block.
- **Reuse:** `TaxIdentifier`, `EmailAddress`, and `PhoneNumber` are NOT duplicated. They will be composed alongside these new VOs in Issue #007.

## Regulatory Sources Used
- **MH JSON Schema `fe-f-v2.json`**: `emisor` block (Lines 156–274), `receptor` block (Lines 275–399).
- **MH Catalogs**: CAT-012 (Departments), CAT-013 (Municipalities), CAT-019 (Economic Activities) — referenced structurally, not imported.

## Testing Summary
Each Value Object has comprehensive unit tests covering: valid construction (happy path), boundary values (min/max lengths), null/empty/whitespace rejection, overflow rejection, and structural equality verification via `GetAtomicValues()`.

## Follow-up Issues
- **Issue #007**: Compose these VOs into `IssuerSnapshot` and `RecipientSnapshot` and embed them in `FiscalDocument`.
- **Issue #008**: Create Catalog Domain (V1) with `EconomicActivity`, `Department`, `Municipality` entities.
- **T-002**: Add `distrito` field to `Address` after cross-DTE-type analysis.

## Traceability References
- [fe-f-v2.json — emisor](file:///c:/Temp/dte-platform/docs/mh/svfe-json-schemas/v2/fe-f-v2.json) Lines 156–274
- [fe-f-v2.json — receptor](file:///c:/Temp/dte-platform/docs/mh/svfe-json-schemas/v2/fe-f-v2.json) Lines 275–399
- [Issue #006 Specification](file:///C:/Users/z004vfcd/.gemini/antigravity-ide/brain/4789ed83-1880-4dac-b209-74b7ec834c05/issue-006-counterparty-identity-model.md)
