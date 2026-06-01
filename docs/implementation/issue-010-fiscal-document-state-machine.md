# Issue #010: Fiscal Document State Machine

**Status:** Completed  
**Date:** 2026-05-31  

---

## 1. Issue #010 Title

**Fiscal Document State Machine**

---

## 2. Objective

Evolve the `FiscalDocument` aggregate from a static structural container into a formal State Machine that governs the complete lifecycle of a Documento Tributario Electrónico (DTE).

Currently, documents exist only in the `Draft` state. By introducing a formal state machine, the domain will enforce that documents move cleanly from `Draft` to `Validated`, `Signed`, and ultimately to a **terminal state** (`Processed` or `Rejected`) assigned by the Ministerio de Hacienda (MH), while strictly preventing modifications (such as adding or removing lines) outside of the `Draft` state.

---

## 3. Scope

1. **State Expansion:** Expand the `FiscalDocumentStatus` enumeration to cover the full transmission lifecycle.
2. **State Transitions:** Introduce explicit, intention-revealing methods on `FiscalDocument` to mutate the document's state.
3. **Transition Guards:** Enforce strict linear transitions (e.g., cannot transition to `Signed` unless currently `Validated`).
4. **Terminal State Enforcement:** `Processed` and `Rejected` are **terminal states** — no further transitions are permitted once reached.
5. **Lifecycle Attributes:** Introduce Value Objects to hold lifecycle milestones, specifically the MH Sello de Recepción and digital signature references.
6. **Validation Gating:** Link the output of the previously implemented `Validate()` method (Issue #009) to the `Draft → Validated` transition.

---

## 4. Out of Scope

- **JSON Payload Generation:** Creating the actual JSON string representation of the DTE.
- **Digital Signing Algorithms:** The actual execution of cryptographic signing (Firmador).
- **MH API Communication:** HTTP clients or logic to transmit documents to Hacienda.
- **Document Invalidation:** The specific "Anulación" event and schema (Eventos DTE) are out of scope for the core generation lifecycle and will be addressed in a separate issue.
- **Infrastructure / Persistence:** Saving state changes to the database.

---

## 5. Domain Changes

### 5.1 `FiscalDocumentStatus` (Update)

**File:** `src/DTE.Domain/Aggregates/FiscalDocuments/FiscalDocumentStatus.cs`

```csharp
public enum FiscalDocumentStatus
{
    Draft = 1,
    Validated = 2,    // Passed all domain validation rules
    Signed = 3,       // Digital signature applied
    Processed = 4,    // TERMINAL — Accepted by MH (Sello de Recepción issued)
    Rejected = 5      // TERMINAL — Rejected by MH
}
```

### 5.2 State Machine Diagram

```text
              ┌─────────┐
              │  Draft   │
              └────┬─────┘
                   │ MarkAsValidated()
                   │ [Validate().IsValid == true]
                   ▼
              ┌──────────┐
              │ Validated │
              └────┬──────┘
                   │ MarkAsSigned(signature)
                   ▼
              ┌─────────┐
              │  Signed  │
              └──┬────┬──┘
                 │    │
    MarkAsProcessed()  MarkAsRejected()
                 │    │
                 ▼    ▼
         ┌──────────┐  ┌──────────┐
         │ Processed │  │ Rejected │
         │ TERMINAL  │  │ TERMINAL │
         └──────────┘  └──────────┘
```

**Terminal State Semantics:** Once a document reaches `Processed` or `Rejected`, **no method on the aggregate may further mutate its state or its structural content**. Any attempt to call a transition method or a mutation method (`AddLine`, `RemoveLine`) on a terminal-state document must return a `Result.Failure`.

### 5.3 New Value Objects

Create the following Value Objects in `src/DTE.Domain/ValueObjects/`:

#### 5.3.1 `DocumentSignature` (Opaque Value)

**File:** `src/DTE.Domain/ValueObjects/DocumentSignature.cs`

Represents the digital signature applied to the document payload. The Domain treats this as an **opaque value** — it stores the signature but does not interpret, validate, or parse its contents. The format and cryptographic validity are the responsibility of the infrastructure layer (Firmador).

```csharp
public sealed class DocumentSignature : ValueObject
{
    public string Value { get; }

    private DocumentSignature(string value) { Value = value; }

    public static Result<DocumentSignature> Create(string value)
    {
        // Guard: value must not be null or whitespace.
        // No format validation — opaque by design.
    }
}
```

**Design Decision:** The Domain does not validate signature format, length, or encoding. It only ensures the value is non-empty. Cryptographic correctness is an infrastructure concern.

#### 5.3.2 `ReceptionStamp` (Opaque MH-Issued Value)

**File:** `src/DTE.Domain/ValueObjects/ReceptionStamp.cs`

Represents the `selloRecibido` issued by the Ministerio de Hacienda upon successful processing of the DTE. The Domain treats this as an **opaque MH-issued value** — it is received from the MH response and stored verbatim. The Domain does not interpret, parse, or validate the stamp's structure.

```csharp
public sealed class ReceptionStamp : ValueObject
{
    public string Value { get; }

    private ReceptionStamp(string value) { Value = value; }

    public static Result<ReceptionStamp> Create(string value)
    {
        // Guard: value must not be null or whitespace.
        // No format validation — opaque MH-issued value.
    }
}
```

**Design Decision:** The `selloRecibido` format is defined and owned by MH. Any attempt to validate its structure in the Domain would couple the Domain to MH implementation details that may change without notice.

#### 5.3.3 `RejectionReason` (Constrained Value)

**File:** `src/DTE.Domain/ValueObjects/RejectionReason.cs`

Captures the reason why MH rejected the document. Unlike the opaque values above, `RejectionReason` carries structured information needed for human-readable diagnostics.

```csharp
public sealed class RejectionReason : ValueObject
{
    public string Code { get; }
    public string Description { get; }

    private RejectionReason(string code, string description)
    {
        Code = code;
        Description = description;
    }

    public static Result<RejectionReason> Create(string code, string description)
    {
        // Guard: code must not be null or whitespace.
        // Guard: description must not be null or whitespace.
        // Guard: description must not exceed 500 characters.
    }
}
```

**Constraints for `RejectionReason.Description`:**

| Property      | Constraint                        | Rationale                                                                 |
|---------------|-----------------------------------|---------------------------------------------------------------------------|
| `Code`        | Non-null, non-whitespace          | Machine-readable identifier for the rejection cause.                      |
| `Description` | Non-null, non-whitespace          | Human-readable explanation required for audit trails and user feedback.    |
| `Description` | Maximum 500 characters            | Prevents unbounded text storage; sufficient for MH rejection messages.    |

### 5.4 `FiscalDocument` Aggregate Additions

**New Properties:**

```csharp
public DocumentSignature? Signature { get; private set; }
public ReceptionStamp? ReceptionStamp { get; private set; }
public RejectionReason? RejectionReason { get; private set; }
```

**New Transition Methods:**

```csharp
// 1. Draft -> Validated
public ValidationResult MarkAsValidated()
{
    // Guard: Status == Draft (if not, throw or return a ValidationResult with a single error)
    // Action:
    //   var result = Validate();
    //   if (result.IsValid) { Status = FiscalDocumentStatus.Validated; }
    //   return result;
}

// 2. Validated -> Signed
public Result MarkAsSigned(DocumentSignature signature)
{
    // Guard: Status == Validated
    // Guard: signature must not be null
    // Action: Assign Signature, Status = FiscalDocumentStatus.Signed
}

// 3. Signed -> Processed (TERMINAL)
public Result MarkAsProcessed(ReceptionStamp stamp)
{
    // Guard: Status == Signed
    // Guard: stamp must not be null
    // Action: Assign ReceptionStamp, Status = FiscalDocumentStatus.Processed
}

// 4. Signed -> Rejected (TERMINAL)
public Result MarkAsRejected(RejectionReason reason)
{
    // Guard: Status == Signed
    // Guard: reason must not be null
    // Action: Assign RejectionReason, Status = FiscalDocumentStatus.Rejected
}
```

**Modification Guards:**
Ensure existing mutation methods (`AddLine`, `RemoveLine`) strictly return a `Result.Failure` if `Status != FiscalDocumentStatus.Draft`. This logic is already present but must be verified against the new enum values. Documents in `Validated`, `Signed`, `Processed`, or `Rejected` status are structurally immutable.

---

## 6. Domain Events Decision

### Decision: **DEFERRED**

> [!IMPORTANT]
> Lifecycle domain events (e.g., `FiscalDocumentValidated`, `FiscalDocumentSigned`, `FiscalDocumentProcessed`, `FiscalDocumentRejected`) are **explicitly deferred** from this issue.

**Rationale:**

1. **No consumer exists.** The repository currently has no infrastructure to dispatch or handle domain events beyond the existing `FiscalDocumentCreated` event. Introducing lifecycle events without a consumer would be speculative.

2. **Event payload design depends on infrastructure.** The payload for `FiscalDocumentSigned` and `FiscalDocumentProcessed` would need to include signature and stamp values respectively. The optimal shape of these payloads depends on the serialization and transmission infrastructure (Issue #010+), which does not yet exist.

3. **Single Responsibility.** This issue's responsibility is the state machine mechanics — guards, transitions, and terminal state enforcement. Adding event design would expand the scope and introduce review surface area unrelated to the core objective.

4. **Existing pattern is preserved.** The `FiscalDocumentCreated` event raised during `Create()` remains unchanged. The `RaiseDomainEvent()` infrastructure on `AggregateRoot` is already available and will be used when lifecycle events are introduced.

**Follow-up:** A dedicated issue will define lifecycle events once an event consumer (e.g., outbox pattern, integration events) is designed. The anticipated events are:

| Event                          | Trigger                  | Key Payload                        |
|--------------------------------|--------------------------|------------------------------------|
| `FiscalDocumentValidated`      | `MarkAsValidated()`      | `DocumentId`, `DteType`            |
| `FiscalDocumentSigned`         | `MarkAsSigned()`         | `DocumentId`, `DocumentSignature`  |
| `FiscalDocumentProcessed`      | `MarkAsProcessed()`      | `DocumentId`, `ReceptionStamp`     |
| `FiscalDocumentRejected`       | `MarkAsRejected()`       | `DocumentId`, `RejectionReason`    |

---

## 7. Tests Required

### 7.1 Value Object Tests

- **DocumentSignature:** `Create` succeeds with non-empty string; fails with null, empty, or whitespace.
- **ReceptionStamp:** `Create` succeeds with non-empty string; fails with null, empty, or whitespace.
- **RejectionReason:** `Create` succeeds with valid code and description; fails when either is null/whitespace; fails when description exceeds 500 characters.

### 7.2 State Transition Tests

- **Happy Path (Processed):** `Draft` → `Validated` → `Signed` → `Processed` succeeds.
- **Happy Path (Rejected):** `Draft` → `Validated` → `Signed` → `Rejected` succeeds.
- **Invalid Transition:** `MarkAsSigned()` on a `Draft` document returns failure.
- **Invalid Transition:** `MarkAsProcessed()` on a `Draft` or `Validated` document returns failure.
- **Invalid Transition:** `MarkAsRejected()` on a `Draft` or `Validated` document returns failure.

### 7.3 Terminal State Tests

- **Processed is terminal:** Calling `MarkAsSigned()`, `MarkAsProcessed()`, `MarkAsRejected()`, `AddLine()`, or `RemoveLine()` on a `Processed` document returns failure.
- **Rejected is terminal:** Calling `MarkAsSigned()`, `MarkAsProcessed()`, `MarkAsRejected()`, `AddLine()`, or `RemoveLine()` on a `Rejected` document returns failure.

### 7.4 Validation Gate Tests

- **Invalid Document:** Calling `MarkAsValidated()` on a structurally invalid document (e.g., no lines) returns an invalid `ValidationResult` and leaves the `Status` as `Draft`.
- **Valid Document:** Calling `MarkAsValidated()` on a valid document returns a valid `ValidationResult` and changes the `Status` to `Validated`.
- **Already Validated:** Calling `MarkAsValidated()` on a `Validated` document returns failure (not `Draft`).

### 7.5 Immutability Tests

- **Locked State:** Calling `AddLine` or `RemoveLine` on a document with status `Validated`, `Signed`, `Processed`, or `Rejected` returns a failure (`InvalidStateForModification`).

---

## 8. Documentation Required

- This specification artifact: `docs/implementation/issue-010-fiscal-document-state-machine.md`.

---

## 9. Regulatory Sources

- **MH Normativa de Cumplimiento de los DTE (Versión 2.0)**: Establishes the asynchronous/synchronous reception flow, dictating that a transmitted document receives either a `selloRecibido` (approved) or is rejected.
- **Manual Técnico para la Integración Tecnológica del Sistema de Transmisión v2**: Defines the flow of generating the JSON, signing the JSON (Firmador), and submitting the JSON payload, which naturally map to `Validated`, `Signed`, and `Processed`/`Rejected` states.
- **svfe-json-schemas/v3/invalidacion-schema-v3.json**: References `selloRecibido` as a required property on processed documents, confirming the reception stamp is an MH-issued artifact.

---

## 10. Acceptance Criteria

1. `FiscalDocumentStatus` contains all 5 required states (`Draft`, `Validated`, `Signed`, `Processed`, `Rejected`).
2. `Processed` and `Rejected` are enforced as **terminal states** — no further state transitions or structural mutations are permitted.
3. `DocumentSignature` is an opaque Value Object: non-empty string, no format validation.
4. `ReceptionStamp` is an opaque MH-issued Value Object: non-empty string, no format validation.
5. `RejectionReason` enforces: `Code` non-empty, `Description` non-empty, `Description.Length ≤ 500`.
6. `MarkAsValidated()` bridges the Notification Pattern (`ValidationResult`) with state mutation, only advancing the state if zero errors are found.
7. Linear state transitions are strictly enforced by aggregate methods returning `Result<T>` or `Result` fail-fast errors on invalid transitions.
8. The document resists modifications to its structural content (`Lines`) once it has moved past the `Draft` state.
9. Domain events are explicitly **deferred** — no lifecycle events are raised by transition methods in this issue.
10. All Domain unit tests pass.
11. Architecture tests pass, proving the Domain Layer maintains no infrastructure dependencies during state mutation.
