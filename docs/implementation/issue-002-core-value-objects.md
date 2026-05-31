# Issue #002: Core Value Objects

**Status:** Completed  
**Date:** 2026-05-31  

## Objective
Establish the immutable domain language that will be used by future aggregates representing core fiscal concepts. This sets up the foundations for FiscalDocument logic in full alignment with the Canonical Domain Specification.

## Scope
Creation of the following immutable `ValueObject` types with invariants logic:
- DocumentId
- ControlNumber
- GenerationCode
- TaxIdentifier
- SchemaVersion
- DteType
- Money
- EmailAddress
- PhoneNumber

## Out of Scope
- Aggregates: FiscalDocument, Counterparty, DocumentLine.
- Application logic, databases, commands, queries.

## Files Created/Modified
- `src/DTE.Domain/ValueObjects/DocumentId.cs`: Value object enforcing Guid identity.
- `src/DTE.Domain/ValueObjects/ControlNumber.cs`: Enforces `DTE-XX-YYYYYYYY-ZZZZZZZZZZZZZZZ` formatting.
- `src/DTE.Domain/ValueObjects/GenerationCode.cs`: Parses and wraps valid UUIDs.
- `src/DTE.Domain/ValueObjects/TaxIdentifier.cs`: Captures specific identity kinds (NIT, NRC).
- `src/DTE.Domain/ValueObjects/SchemaVersion.cs`: Simple version enforcement.
- `src/DTE.Domain/ValueObjects/DteType.cs`: Constrains to allowed '01' (FE) and '03' (CCF).
- `src/DTE.Domain/ValueObjects/Money.cs`: Enforces non-negative values and USD restrictions.
- `src/DTE.Domain/ValueObjects/EmailAddress.cs`: Email Regex validation.
- `src/DTE.Domain/ValueObjects/PhoneNumber.cs`: Phone Regex validation.
- `tests/DTE.Domain.Tests/ValueObjects/*`: All corresponding unit tests covering creation invariants.

## Architectural Decisions Used
- Value Objects encapsulate all rule logic statically with `Create()` methods returning `Result<T>` rather than throwing exceptions, enforcing the approved Functional error approach.
- Immutability guarantees are handled via structurally protected classes utilizing explicit `ValueObject` sequences.

## Implementation Summary
The core primitives have been defined. `Money` structurally prevents non-USD assignment and negative valuations. `DteType` strictly limits construction to Factura Electrónica and Comprobante Crédito Fiscal. Complex serialization and strict format requirements are encapsulated safely within `ControlNumber` and `EmailAddress`/`PhoneNumber`. 

## Testing Summary
Every created ValueObject is tested in `DTE.Domain.Tests`. Tests explicitly verify that correct formats construct successfully containing the precise provided values, while malformed inputs (empty fields, missing specific structural regex compliance, unsupported codes) yield accurate `IsFailure` states and specific error codes.

## Follow-up Issues
- **Issue #003**: Definition of the `Counterparty` and `Item` master data aggregates using these newly forged Value Objects.
- **Issue #004**: Creation of the primary `FiscalDocument` aggregate.

## Traceability References
- [Core Value Objects Money & Snapshots.pdf](file:///c:/Temp/dte-platform/docs/Core%20Value%20Objects%20Money%20%26%20Snapshots.pdf)
- [DTE_Platform__V1_Domain_Specification_(Canonical).pdf](file:///c:/Temp/dte-platform/docs/DTE_Platform__V1_Domain_Specification_%28Canonical%29.pdf)
