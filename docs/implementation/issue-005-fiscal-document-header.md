# Issue #005: Fiscal Document Header

**Status:** Completed  
**Date:** 2026-05-31  

## Objective
Extend the `FiscalDocument` aggregate with the minimum header information required by the MH schemas, transforming it from a skeletal structure into a meaningful fiscal document representation.

## Scope
Implementation of strictly supported header fields:
- `DocumentVersion`
- `EnvironmentType`
- `OperationType`
- `IssueDate`
- `IssueTime`

## Out of Scope
- Document lines, taxes, totals, counterparty data.
- Transmission, signing, and invalidation validations.

## Files Created/Modified
- `src/DTE.Domain/Aggregates/FiscalDocuments/EnvironmentType.cs`: Enum mapping `00` (Test) and `01` (Production) environments.
- `src/DTE.Domain/Aggregates/FiscalDocuments/OperationType.cs`: Enum mapping `1` (Normal) and `2` (Contingency) operations.
- `src/DTE.Domain/Aggregates/FiscalDocuments/FiscalDocument.cs`: Modified to encapsulate and validate the new header properties.
- `src/DTE.Domain/Events/FiscalDocumentCreated.cs`: Modified to broadcast the header state upon creation.
- `tests/DTE.Domain.Tests/Aggregates/FiscalDocuments/FiscalDocumentTests.cs`: Updated unit tests verifying valid header creation and constraints.

## Regulatory Sources Used
- **MH JSON Schemas (`svfe-json-schemas/v1` & `v2`)**: The `identificacion` block specifies fields like `version`, `ambiente`, `tipoOperacion`, `fecEmi`, and `horEmi` as mandatory components of every valid FE and CCF payload.
- **Canonical Domain Model**: Ensures alignment with the approved `FiscalDocument` boundary.

## Validation Rules
- `DocumentVersion` must strictly be greater than zero.
- Properties must be supplied correctly via the `FiscalDocument.Create()` factory to guarantee atomic aggregate initialization.

## Test Coverage
- Verified successful factory construction parsing specific `DateOnly` and `TimeOnly` elements.
- Confirmed failure on `DocumentVersion` <= 0.
- Confirmed payload fidelity passes directly into the `FiscalDocumentCreated` event correctly.

## Follow-up Dependencies
- **Issue #006**: Implementation of the `Counterparty` identity block (Issuer and Receptor snapshots) into the `FiscalDocument`.
