# Issue #004: Control Number Series Aggregate

**Status:** Completed  
**Date:** 2026-05-31  

## Objective
Implement the `ControlNumberSeries` aggregate root to govern fiscal control number allocation. This ensures the foundational business rules of uniqueness, sequential monotonic ordering, and gapless tracking of correlatives per Point of Sale and DTE Type.

## Scope
- Creation of `ControlNumberSeries` Aggregate Root.
- Implementation of sequence tracking via `NextCorrelative`.
- Development of the `AllocateNext()` method.
- Dispatch of `ControlNumberSeriesCreated` and `ControlNumberAllocated` domain events.
- Aggregate invariants validating identity formats.

## Out of Scope
- Actually assigning the `ControlNumber` to a `FiscalDocument`.
- Repositories, Commands, API Controllers.
- Concurrency logic (deferred to Application/Infrastructure layers using DB constraints or distributed locks).

## Files Created
- `src/DTE.Domain/Aggregates/ControlNumberSeries/ControlNumberSeries.cs`: The Aggregate Root protecting correlative logic.
- `src/DTE.Domain/Events/ControlNumberSeriesCreated.cs`: Domain event dispatched on initialization.
- `src/DTE.Domain/Events/ControlNumberAllocated.cs`: Domain event capturing each assigned token.
- `tests/DTE.Domain.Tests/Aggregates/ControlNumberSeries/ControlNumberSeriesTests.cs`: Validation test coverage.

## Architectural Decisions Used
- **Encapsulated State Mutation:** The `NextCorrelative` integer is completely guarded. It can only be mutated through the successful execution of `AllocateNext()`, preventing any manual skips or rewinds.
- **Strict Formatting Delegation:** The aggregate leverages the trusted `ControlNumber.Create` factory method to ensure the allocated string structurally complies with the expected El Salvador format (`DTE-XX-YYYYYYYY-ZZZZZZZZZZZZZZZ`), reusing previous issue domain rules.

## Implementation Summary
`ControlNumberSeries` tracks the `DteType`, `EstablishmentCode`, `PosCode`, and `NextCorrelative`. Instantiating the series via `Create` validates that the combined issuing point lengths correctly match the 8-character strict requirement enforced by the MH regex. Subsequent calls to `AllocateNext()` atomically generate the correctly padded string token, safely advance the internal integer, and publish an allocation event.

## Testing Summary
Comprehensive unit tests cover successful series creation, verification of length restrictions on POS codes, and sequential allocation logic. Tests ensure that consecutive calls to `AllocateNext` successfully increment the trailing correlative sequence without rewinding, matching the strict output format required by `ControlNumber`.

## Follow-up Issues
- **Issue #005**: Application-level commands orchestrating the validation transition of a `FiscalDocument`, utilizing this `ControlNumberSeries` to draw identity and apply it via concurrency-safe infrastructure.

## Traceability References
- [Core Value Objects Money & Snapshots.pdf](file:///c:/Temp/dte-platform/docs/Core%20Value%20Objects%20Money%20%26%20Snapshots.pdf)
- [DTE_Platform__V1_Domain_Specification_(Canonical).pdf](file:///c:/Temp/dte-platform/docs/DTE_Platform__V1_Domain_Specification_%28Canonical%29.pdf)
