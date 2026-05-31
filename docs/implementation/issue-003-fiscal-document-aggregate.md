# Issue #003: Fiscal Document Aggregate

**Status:** Completed  
**Date:** 2026-05-31  

## Objective
Implement the FiscalDocument Aggregate Root. This aggregate represents the central business concept of the platform and enforces lifecycle integrity and invariants defined by the approved Domain Specification.

## Scope
- Creation of `FiscalDocument` Aggregate Root.
- Creation of `FiscalDocumentStatus` (limited to `Draft`).
- Implementation of Draft lifecycle behavior.
- Aggregate invariants for construction.
- Domain event `FiscalDocumentCreated` dispatching upon creation.

## Out of Scope
- Transmission lifecycle, MH integration, invalidation lifecycle, contingency lifecycle.
- Commands, Queries, Repositories, API Controllers.
- Signatures and Validation transitions.

## Files Created
- `src/DTE.Domain/Aggregates/FiscalDocuments/FiscalDocumentStatus.cs`: Enumeration for status tracking.
- `src/DTE.Domain/Events/FiscalDocumentCreated.cs`: Domain event dispatched on valid creation.
- `src/DTE.Domain/Aggregates/FiscalDocuments/FiscalDocument.cs`: The Aggregate Root protecting internal consistency.
- `tests/DTE.Domain.Tests/Aggregates/FiscalDocuments/FiscalDocumentTests.cs`: Unit tests asserting creation and event rules.

## Architectural Decisions Used
- **Factory Methods:** Construction is strictly controlled by a static `Create()` returning a `Result<FiscalDocument>`, preventing invalid instantiation.
- **Nullability and Invariants:** Optional fields in `Draft` state like `ControlNumber` and `GenerationCode` are modeled as nullable properties (`?`), whereas required fields (`DocumentId`, `DteType`) enforce null checks.
- **Event Sourcing (Memory):** Creation securely pushes the `FiscalDocumentCreated` domain event to the internal `AggregateRoot` list for transactional outbox dispatching later.

## Implementation Summary
The `FiscalDocument` aggregate was successfully constructed. It inherits from `AggregateRoot` (incorporating audit fields via `AuditableEntity`). `FiscalDocument` guarantees it begins in `Draft` status without any `ControlNumber` or `GenerationCode` defined. It encapsulates its own construction rules.

## Testing Summary
Comprehensive unit tests cover successful generation, verification of `FiscalDocumentCreated` domain event emission, and negative scenarios preventing instantiation with null dependent Value Objects (`DocumentId` and `DteType`).

## Follow-up Issues
- **Issue #004**: Implementation of Document Validation transition logic.
- **Issue #005**: Integration of `FiscalDocument` lines and tax summary aggregations.

## Traceability References
- [DTE_Platform__V1_Domain_Specification_(Canonical).pdf](file:///c:/Temp/dte-platform/docs/DTE_Platform__V1_Domain_Specification_%28Canonical%29.pdf)
