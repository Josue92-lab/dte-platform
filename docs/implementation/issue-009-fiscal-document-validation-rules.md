# Issue #009: Fiscal Document Validation Rules

**Status:** Completed  
**Date:** 2026-05-31  

---

## 1. Executive Summary

This issue introduces domain-level validation into the `FiscalDocument` aggregate. Today, a `FiscalDocument` can be constructed and populated with lines, but the Domain cannot answer the question: **"Is this document valid and ready for transmission?"**

The design introduces a **Notification Pattern** via `ValidationResult` — a collection of domain errors — combined with **aggregate self-validation** through a new `Validate()` method on `FiscalDocument`. Validation rules are explicit, statically coded methods inside the aggregate boundary. No external services, databases, rule engines, or plugin systems are required.

The validation architecture is designed to be DTE-type-aware from inception, supporting both Factura Electrónica (FE, code `01`) and Comprobante de Crédito Fiscal (CCF, code `03`) rules within a single aggregate, differentiated by the existing `DteType` value.

---

## 2. Problem Statement

The `FiscalDocument` aggregate currently enforces:

- **Construction-time guards:** Null checks on `DocumentId`, `DteType`, `Issuer`, etc.
- **Mutation-time guards:** Status check, line-limit enforcement, and null VO guards in `AddLine`.
- **Mathematical invariants:** `DocumentTotals` is always the exact sum of lines.

However, the aggregate **cannot** answer whether its overall state constitutes a valid fiscal document. Specifically:

- Can a document with zero lines be considered valid?
- Does a CCF require a fully identified recipient while an FE does not?
- Are there negative totals that should be rejected?
- Is the issue date in the future?
- Does a line have all-zero monetary amounts (economically meaningless)?

Without an explicit validation gate, the Application layer must duplicate business knowledge to decide whether a document is ready. This violates the principle that the domain must own its own invariants.

---

## 3. Architectural Context

### 3.1 Current Aggregate State

```text
┌───────────────────────────────────────────────────────────┐
│                   FiscalDocument                          │
│                                                           │
│  DocumentId, DteType, Status, ControlNumber?,             │
│  GenerationCode?, DocumentVersion, EnvironmentType,       │
│  OperationType, IssueDate, IssueTime                      │
│                                                           │
│  IssuerSnapshot  (required)                               │
│  RecipientSnapshot? (optional)                            │
│                                                           │
│  Lines: IReadOnlyCollection<DocumentLine>                 │
│  Totals: DocumentTotals  (computed)                       │
│                                                           │
│  ┌─────────────────────────────────────────────┐          │
│  │  MISSING:  Validate() → ValidationResult    │  ← #009 │
│  └─────────────────────────────────────────────┘          │
└───────────────────────────────────────────────────────────┘
```

### 3.2 Validation Architecture Evaluation

Three approaches were evaluated:

#### A. Notification Pattern ← **SELECTED**

The aggregate exposes `ValidationResult Validate()` which collects **all** violations in a single pass and returns them as a list. The caller receives a complete diagnostic rather than failing on the first error.

**Advantages:**
- Complete error reporting in a single call.
- Natural fit for form validation, API responses, and UI feedback.
- No coupling to external services.
- The aggregate owns its validation logic.

**Disadvantages:**
- Slightly more complex than single-error `Result<T>`.

#### B. Validation Service (Domain Service)

A separate `FiscalDocumentValidator` service that accepts a `FiscalDocument` and returns errors.

**Rejected because:**
- Validation operates exclusively on aggregate state — there is no cross-aggregate or external data requirement.
- Extracting validation into a service splits the aggregate's invariant knowledge across two locations.
- Violates "Tell, Don't Ask" — the service would need to interrogate every property.

#### C. Aggregate Self-Validation via `Result<T>` (Fail-Fast)

Modify `FiscalDocument` to expose a `Result Validate()` that returns the first error only.

**Rejected because:**
- In a fiscal document context, callers need to know *all* violations to fix a draft, not just the first one.
- Would require repeated validate-fix-validate cycles, degrading UX and API ergonomics.

### 3.3 Justification for Selected Approach

The **Notification Pattern with Aggregate Self-Validation** is selected because:

1. Validation rules operate exclusively on aggregate-internal state.
2. The aggregate is the single source of truth for its own validity.
3. The Notification Pattern provides complete diagnostics in one call.
4. No new abstractions beyond `ValidationResult` and `ValidationError` are introduced.
5. The pattern naturally extends to DTE-type-specific rules via method dispatch on `DteType`.

---

## 4. Scope

1. **Create `ValidationError` record** — Lightweight error descriptor with code, message, and optional field path.
2. **Create `ValidationResult` class** — Notification-pattern container that accumulates zero or more `ValidationError` instances.
3. **Add `Validate()` method to `FiscalDocument`** — Returns a `ValidationResult` by executing all applicable rules.
4. **Implement universal validation rules** — Rules that apply to all DTE types.
5. **Implement FE-specific validation rules** — Rules for `DteType.FacturaElectronica`.
6. **Implement CCF-specific validation rules** — Rules for `DteType.ComprobanteCreditoFiscal`.

---

## 5. Out of Scope

- **MH API validation:** Server-side schema validation performed by Ministerio de Hacienda.
- **JSON schema validation:** Infrastructure-level structural checks.
- **Catalog validation:** Verifying that `UnitOfMeasure` codes, `DepartmentCode`, etc. exist in MH catalogs.
- **Catalog synchronization.**
- **Digital signatures / Sello de recepción.**
- **Transmission / Contingency.**
- **Persistence / EF Core / Repositories.**
- **Audit trails.**
- **State transitions:** Whether `Validate()` should gate a status change (e.g., Draft → Validated) is deferred. This issue only establishes the validation capability.

---

## 6. Domain Model Changes

### 6.1 `ValidationError` (New Record)

**File:** `src/DTE.Domain/Primitives/ValidationError.cs`

```csharp
public sealed record ValidationError(string Code, string Message, string? Field = null);
```

| Property | Type     | Purpose                                              |
|----------|----------|------------------------------------------------------|
| `Code`   | `string` | Machine-readable error identifier                    |
| `Message`| `string` | Human-readable description                           |
| `Field`  | `string?`| Optional dot-path to the violating property (e.g., `"Totals.TotalToPay"`) |

**Design Note:** `ValidationError` is distinct from the existing `Error` record. `Error` is used in `Result<T>` for single-error fail-fast flows. `ValidationError` is used in `ValidationResult` for multi-error Notification flows. They serve different design patterns and must not be conflated.

### 6.2 `ValidationResult` (New Class)

**File:** `src/DTE.Domain/Primitives/ValidationResult.cs`

```csharp
public sealed class ValidationResult
{
    private readonly List<ValidationError> _errors = new();

    public IReadOnlyCollection<ValidationError> Errors => _errors.AsReadOnly();
    public bool IsValid => _errors.Count == 0;

    internal void AddError(ValidationError error) => _errors.Add(error);

    public static ValidationResult Valid() => new();
}
```

**Why `internal AddError`:** Only the aggregate and its validation methods may append errors. External callers receive a read-only result.

**Why a class, not a struct:** `ValidationResult` is mutable during the validation pass (errors are appended), then frozen on return. A struct with a mutable list would introduce copy semantics issues.

### 6.3 `FiscalDocument.Validate()` (New Method)

**File:** `src/DTE.Domain/Aggregates/FiscalDocuments/FiscalDocument.cs`

```csharp
public ValidationResult Validate()
{
    var result = new ValidationResult();

    ValidateUniversalRules(result);

    if (DteType == DteType.FacturaElectronica)
        ValidateFacturaElectronicaRules(result);
    else if (DteType == DteType.ComprobanteCreditoFiscal)
        ValidateCreditoFiscalRules(result);

    return result;
}
```

**Why `if/else` instead of polymorphism:** The `FiscalDocument` aggregate is a single sealed class — DTE types are value objects, not subclasses. Method dispatch on `DteType` keeps validation local to the aggregate without introducing inheritance hierarchies or strategy objects.

---

## 7. Validation Rules

### 7.1 Universal Rules (All DTE Types)

| Rule ID | Code                                          | Condition                                                             | Field                  |
|---------|-----------------------------------------------|-----------------------------------------------------------------------|------------------------|
| U-01    | `Validation.DocumentMustHaveLines`            | `Lines.Count == 0`                                                    | `Lines`                |
| U-02    | `Validation.IssuerRequired`                   | `Issuer is null`                                                      | `Issuer`               |
| U-03    | `Validation.TotalToPayMustNotBeNegative`      | `Totals.TotalToPay.Amount < 0`                                       | `Totals.TotalToPay`    |
| U-04    | `Validation.SubTotalSalesMustNotBeNegative`   | `Totals.SubTotalSales.Amount < 0`                                    | `Totals.SubTotalSales` |
| U-05    | `Validation.IssueDateRequired`                | `IssueDate == DateOnly.MinValue`                                      | `IssueDate`            |
| U-06    | `Validation.DocumentVersionRequired`          | `DocumentVersion <= 0`                                                | `DocumentVersion`      |
| U-07    | `Validation.LineHasNoEconomicValue`           | A line exists where `NonTaxableAmount + ExemptAmount + TaxableAmount` equals `Money.Zero` | `Lines[n]`             |
| U-08    | `Validation.NumItemSequenceInvalid`           | `Lines` are not sequentially numbered `1..N`                          | `Lines`                |

### 7.2 Factura Electrónica (FE) Rules

| Rule ID | Code                                          | Condition                                                             | Field                  |
|---------|-----------------------------------------------|-----------------------------------------------------------------------|------------------------|
| FE-01   | `Validation.FE.DteTypeMismatch`               | `DteType != DteType.FacturaElectronica` (guard clause)                | `DteType`              |

**Note:** FE permits a null `Recipient`. No additional recipient constraints beyond the universal rules. The FE receptor block in the MH schema allows all fields to be `null`.

### 7.3 Comprobante de Crédito Fiscal (CCF) Rules

| Rule ID | Code                                          | Condition                                                             | Field                  |
|---------|-----------------------------------------------|-----------------------------------------------------------------------|------------------------|
| CCF-01  | `Validation.CCF.RecipientRequired`            | `Recipient is null`                                                   | `Recipient`            |
| CCF-02  | `Validation.CCF.RecipientIdentifierRequired`  | `Recipient.DocumentIdentifier is null`                                | `Recipient.DocumentIdentifier` |
| CCF-03  | `Validation.CCF.RecipientAddressRequired`     | `Recipient.Address is null`                                           | `Recipient.Address`    |
| CCF-04  | `Validation.CCF.RecipientEconomicActivityRequired` | `Recipient.EconomicActivity is null`                             | `Recipient.EconomicActivity` |

**Regulatory Source:** [fe-ccf-v4.json](file:///c:/Temp/dte-platform/docs/mh/svfe-json-schemas/v4/fe-ccf-v4.json) (version 4, `tipoDte: "03"`). The CCF receptor block is non-nullable (`type: "object"`) and requires `nit`, `nombre`, `codActividad`, `descActividad`, and `direccion` as non-nullable strings/objects. The fields `nrc`, `nombreComercial`, `telefono`, and `correo` are declared `["string", "null"]` — nullable. The FE schema ([fe-f-v2.json](file:///c:/Temp/dte-platform/docs/mh/svfe-json-schemas/v2/fe-f-v2.json)) permits the entire receptor block to be null (`type: ["object", "null"]`).

---

## 8. Error Modeling

All validation errors use the `ValidationError` record. Error codes follow the convention:

```text
Validation.<RuleDescription>         — universal rules
Validation.FE.<RuleDescription>      — FE-specific rules
Validation.CCF.<RuleDescription>     — CCF-specific rules
```

This convention:
- Is grep-friendly across the codebase.
- Naturally maps to API error responses.
- Distinguishes validation errors from construction/mutation errors (`FiscalDocument.*`, `Money.*`, etc.).

---

## 9. Aggregate Responsibilities

| Responsibility                     | Owner                | How                                                      |
|------------------------------------|----------------------|----------------------------------------------------------|
| Construction-time null guards      | `FiscalDocument.Create()` | Existing `Result<T>` pattern — fail fast              |
| Mutation-time invariant guards     | `AddLine()` / `RemoveLine()` | Existing `Result<T>` pattern — fail fast           |
| Totals mathematical integrity      | `RecalculateTotals()` | Automatic — no external trigger                         |
| **Document-level completeness**    | **`Validate()`**     | **Notification pattern — collect all errors** ← NEW     |

**Key Design Principle:** `Validate()` is a **query**, not a command. It does not mutate the aggregate. It does not change `Status`. It does not raise domain events. It simply inspects the current state and reports violations.

---

## 10. Acceptance Criteria

### Universal Rules
- [ ] Calling `Validate()` on a document with zero lines returns `Validation.DocumentMustHaveLines`.
- [ ] Calling `Validate()` on a document where `TotalToPay.Amount < 0` returns `Validation.TotalToPayMustNotBeNegative`.
- [ ] Calling `Validate()` on a document where `SubTotalSales.Amount < 0` returns `Validation.SubTotalSalesMustNotBeNegative`.
- [ ] Calling `Validate()` on a document with a line where all three sale categories sum to zero returns `Validation.LineHasNoEconomicValue`.
- [ ] Calling `Validate()` on a valid FE document with lines returns `IsValid == true`.
- [ ] Calling `Validate()` on a document with multiple violations returns all violations, not just the first.

### FE Rules
- [ ] Calling `Validate()` on a valid FE document with no recipient returns `IsValid == true`.

### CCF Rules
- [ ] Calling `Validate()` on a CCF document with no recipient returns `Validation.CCF.RecipientRequired`.
- [ ] Calling `Validate()` on a CCF document whose recipient lacks `DocumentIdentifier` returns `Validation.CCF.RecipientIdentifierRequired`.
- [ ] Calling `Validate()` on a CCF document whose recipient lacks `Address` returns `Validation.CCF.RecipientAddressRequired`.
- [ ] Calling `Validate()` on a CCF document whose recipient lacks `EconomicActivity` returns `Validation.CCF.RecipientEconomicActivityRequired`.
- [ ] Calling `Validate()` on a valid CCF document with a fully-identified recipient returns `IsValid == true`.
- [ ] Calling `Validate()` on a valid CCF document whose recipient has `Nrc == null` returns `IsValid == true` (NRC is nullable per CCF schema).

### Notification Behavior
- [ ] `ValidationResult.IsValid` returns `true` when no errors are present.
- [ ] `ValidationResult.Errors` is an empty read-only collection when valid.
- [ ] Multiple errors can be accumulated in a single `ValidationResult`.

---

## 11. Definition of Done

1. `ValidationError` record created in `src/DTE.Domain/Primitives/`.
2. `ValidationResult` class created in `src/DTE.Domain/Primitives/`.
3. `FiscalDocument.Validate()` method implemented with universal, FE, and CCF rule dispatch.
4. All validation rules from Section 7 implemented as explicit private methods.
5. Comprehensive unit tests covering every rule and the notification accumulation behavior.
6. All existing tests continue to pass (`dotnet test`).
7. Code complies with formatting standards (`dotnet format --verify-no-changes`).
8. Architecture tests pass.

---

## 12. Testing Requirements

### ValidationResult Tests
- **Empty result is valid:** A freshly created `ValidationResult` has `IsValid == true` and empty `Errors`.
- **Single error makes result invalid:** Adding one error sets `IsValid == false`.
- **Multiple errors are accumulated:** Adding N errors results in `Errors.Count == N`.

### Universal Rule Tests
- **No lines:** Document with zero lines produces `U-01`.
- **Negative TotalToPay:** Discount exceeding sub-total produces `U-03`.
- **Negative SubTotalSales:** Edge case producing `U-04`.
- **Zero-value line:** A line with `NonTaxable=0, Exempt=0, Taxable=0` produces `U-07`.
- **Multiple violations:** Document with zero lines AND negative totals produces both errors.

### FE-Specific Tests
- **Valid FE without recipient:** No recipient is acceptable → `IsValid == true`.
- **Valid FE with recipient:** Both participants → `IsValid == true`.

### CCF-Specific Tests
- **No recipient:** Produces `CCF-01`.
- **Recipient without DocumentIdentifier:** Produces `CCF-02`.
- **Recipient without Address:** Produces `CCF-03`.
- **Recipient without EconomicActivity:** Produces `CCF-04`.
- **Recipient with null NRC:** NRC is nullable per CCF schema → no error produced.
- **Fully valid CCF:** All recipient fields populated → `IsValid == true`.
- **Multiple CCF violations:** Recipient missing both `DocumentIdentifier` and `Address` → both errors.

### Integration Tests (Aggregate-Level)
- **Full valid FE document:** Create document, add lines, validate → `IsValid == true`.
- **Full valid CCF document:** Create document with full recipient, add lines, validate → `IsValid == true`.

---

## 13. Documentation Requirements

- Save this specification to `docs/implementation/issue-009-fiscal-document-validation-rules.md`.
- No other documentation required for this design task.

---

## 14. Future Follow-Up Issues

| Issue    | Title                                        | Dependency on #009                                                    |
|----------|----------------------------------------------|-----------------------------------------------------------------------|
| **#010** | DTE JSON Payload Generation                  | Infrastructure layer should call `Validate()` before serialization.   |
| **#011** | Document State Machine                       | `Validate()` becomes the gate for `Draft → Validated` transition.     |
| **#012** | Validation for Additional DTE Types          | FSE (code 14), Retención (code 07) — extend `Validate()` dispatch.   |
| **T-005**| Validation Performance Benchmarking          | Ensure `Validate()` on a 2000-line document completes under 10ms.     |

---

## Architecture Review Board Verdict

### Classification: **APPROVED**

### Justification

1. **Correct Pattern Selection.** The Notification Pattern is the right choice for fiscal document validation. A taxpayer correcting a draft invoice needs to see *all* errors — not discover them one by one through repeated submissions. The Validation Service alternative was correctly rejected because validation requires no external state.

2. **Aggregate Boundary Integrity.** `Validate()` is a pure query on aggregate state. It introduces no new dependencies, no external services, and no infrastructure coupling. The aggregate remains the single authoritative source for document validity.

3. **DTE-Type Extensibility.** The `if/else` dispatch on `DteType` is appropriate at this stage (2 types). If the system grows to 6+ types, refactoring to a dictionary of validation delegates would be a valid optimization — but premature now. The current design explicitly avoids speculative abstraction.

4. **Clean Separation of Concerns.** Construction guards (`Create`, `AddLine`) prevent structurally invalid state. `Validate()` determines business completeness. These are distinct responsibilities and the specification correctly avoids conflating them.

5. **No Over-Engineering.** The design introduces exactly two new types (`ValidationError`, `ValidationResult`) and one new method (`Validate()`). No rule engines, no strategy patterns, no configuration frameworks. This is the smallest correct design.

6. **Regulatory Traceability.** Each CCF rule directly maps to [fe-ccf-v4.json](file:///c:/Temp/dte-platform/docs/mh/svfe-json-schemas/v4/fe-ccf-v4.json) receptor requirements. The asymmetry between FE (nullable receptor block and fields) and CCF (non-nullable receptor block, with `nit`/`nombre`/`codActividad`/`descActividad`/`direccion` non-nullable but `nrc`/`nombreComercial`/`telefono`/`correo` nullable) is correctly captured.

### Concerns (Non-Blocking)

> [!NOTE]
> **U-07 (LineHasNoEconomicValue):** This rule rejects lines where `NonTaxable + Exempt + Taxable == 0`. While economically meaningless lines should not exist, implementers should verify whether MH permits zero-value lines in edge cases (e.g., promotional items). If so, this rule should be relaxed to a warning in a future iteration.

> [!NOTE]
> **State Transition Deferral:** This specification explicitly defers the question of whether `Validate()` should gate a status transition. Issue #011 must address this. Until then, `Validate()` is advisory — the aggregate does not prevent an invalid document from existing in Draft status.
