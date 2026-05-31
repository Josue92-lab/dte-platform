# Issue #008: Fiscal Document Body

**Status:** Proposed  
**Date:** 2026-05-31  

---

## 1. Executive Summary

This issue establishes the core economic model of the `FiscalDocument` aggregate by introducing line items (`DocumentLine`) and calculated summaries (`DocumentTotals`). It transforms the aggregate from an identity-holding wrapper into a structurally complete representation of an invoice capable of supporting El Salvador's Factura Electrónica (FE) and Comprobante de Crédito Fiscal (CCF) schemas. 

The design strictly favors the "smallest correct model," explicitly avoiding speculative tax engines or pricing frameworks. Instead, the Application layer provides pre-calculated row-level values, and the Domain enforces mathematical integrity by guaranteeing that document totals are exclusively derived by summing the aggregate's lines.

---

## 2. Problem Statement

The `FiscalDocument` currently models its identity (Issue #005) and its participants (`IssuerSnapshot`, `RecipientSnapshot` via Issue #007). However, it lacks the actual economic substance: what is being sold, in what quantities, at what prices, and what the final amounts are.

The MH schema requires:
- `cuerpoDocumento`: A collection of 1 to 2000 line items detailing the transaction.
- `resumen`: A mathematical aggregation of the line items representing total sales, taxes, discounts, and the final amount to pay.

Without these structures, the domain cannot represent a valid electronic document.

---

## 3. Architectural Context

### 3.1 Aggregate Boundaries

```text
┌───────────────────────────────────────────────┐
│               FiscalDocument                  │
│                                               │
│  Header (Issue #005)                          │
│  IssuerSnapshot (Issue #007)                  │
│  RecipientSnapshot (Issue #007)               │
│                                               │
│  DocumentTotals (Value Object) ← COMPUTED     │
│                                               │
│  Lines (Child Entities)                       │
│  ┌─────────────────┐     ┌─────────────────┐  │
│  │ DocumentLine    │     │ DocumentLine    │  │
│  └─────────────────┘     └─────────────────┘  │
└───────────────────────────────────────────────┘
```

**Why `DocumentLine` is an Entity (not a Value Object):**
Line items have local identity (a sequential `NumItem` required by MH) and represent distinct physical or logical components of the transaction that may need to be individually modified or removed during document drafting. They exist only within the boundary of the `FiscalDocument`.

**Why `DocumentTotals` is a Value Object:**
Totals have no identity. They are a purely functional, immutable reflection of the current state of the lines.

---

## 4. Scope

1. **Create `Money` Value Object** — Encapsulates non-negative USD amounts.
2. **Create `Quantity` Value Object** — Encapsulates positive fractional quantities.
3. **Create `DocumentLine` Child Entity** — Represents a single row in the document body.
4. **Create `DocumentTotals` Value Object** — Represents the aggregate `resumen` block.
5. **Modify `FiscalDocument` Aggregate** — Introduce line collection management (`AddLine`, `RemoveLine`) and automatic totals calculation.

---

## 5. Out of Scope

- **Tax & Pricing Engines:** The domain will not determine whether a product is subject to 13% IVA. The caller provides the explicit tax amount.
- **Catalog Domains:** Items will not reference a Product Catalog. Codes remain opaque strings.
- **MH Importers / Transmitters:** Formatting JSON payloads for MH is infrastructure logic.
- **Contingencies:** Offline contingency modes are not modeled here.
- **Persistence:** EF Core configuration, entity mappings, migrations.
- **Audit Trails.**

---

## 6. Domain Design

### 6.1 Calculation Responsibilities

**Row-level math (Application/Caller):**
The aggregate **does not** compute taxes or discounts per line. The caller (Application Layer) is responsible for applying tax rules and calculating `TaxableAmount`, `TaxAmount`, `UnitPrice`, and `Discount` for the row, passing these values to `DocumentLine`.

**Document-level math (Domain):**
The aggregate **does** compute `DocumentTotals`. The totals can **never** be manually set or overridden. They are an absolute invariant calculated directly by summing the values of the existing `DocumentLine` entities.

---

## 7. Aggregate Design: `FiscalDocument`

**New Properties:**
- `IReadOnlyCollection<DocumentLine> Lines { get; }`
- `DocumentTotals Totals { get; }`

**New Behaviors:**
- `Result<DocumentLine> AddLine(...)`: Adds a line item, auto-assigns `NumItem`, and recalculates `Totals`.
- `Result RemoveLine(int numItem)`: Removes a line and recalculates `Totals`.

*Note: Document modifications are only permitted when `Status == FiscalDocumentStatus.Draft`.*

---

## 8. Child Entity Design: `DocumentLine`

**File:** `src/DTE.Domain/Aggregates/FiscalDocuments/DocumentLine.cs`

| Property           | Type          | MH Schema Mapping       |
|--------------------|---------------|-------------------------|
| `NumItem`          | `int`         | `numItem`               |
| `Quantity`         | `Quantity`    | `cantidad`              |
| `UnitOfMeasure`    | `int`         | `uniMedida` (CAT-014)   |
| `Description`      | `string`      | `descripcion`           |
| `UnitPrice`        | `Money`       | `precioUni`             |
| `DiscountAmount`   | `Money`       | `montoDescu`            |
| `NonTaxableAmount` | `Money`       | `ventaNoSuj`            |
| `ExemptAmount`     | `Money`       | `ventaExenta`           |
| `TaxableAmount`    | `Money`       | `ventaGravada`          |
| `TaxAmount`        | `Money`       | `ivaItem`               |

*Note: For V1, complex item properties like `tributos` array, `codigo`, and `psv` are simplified or omitted unless strictly mandatory for mathematical consistency.*

---

## 9. Value Objects

### 9.1 `Money`
**File:** `src/DTE.Domain/ValueObjects/Money.cs`
- `decimal Value { get; }`
- Invariant: `Value >= 0` (negative amounts are not permitted by MH in normal operation; discounts are positive decimals).
- Supports addition (`+`) operator.

### 9.2 `Quantity`
**File:** `src/DTE.Domain/ValueObjects/Quantity.cs`
- `decimal Value { get; }`
- Invariant: `Value > 0`.

### 9.3 `DocumentTotals`
**File:** `src/DTE.Domain/ValueObjects/DocumentTotals.cs`
Represents the structural equivalent of the MH `resumen` block.

| Property               | Derivation (Sum of Lines)                        |
|------------------------|--------------------------------------------------|
| `TotalNonTaxable`      | `Sum(NonTaxableAmount)`                          |
| `TotalExempt`          | `Sum(ExemptAmount)`                              |
| `TotalTaxable`         | `Sum(TaxableAmount)`                             |
| `SubTotalSales`        | `TotalNonTaxable + TotalExempt + TotalTaxable`   |
| `TotalDiscount`        | `Sum(DiscountAmount)`                            |
| `TotalTax`             | `Sum(TaxAmount)`                                 |
| `TotalToPay`           | `SubTotalSales + TotalTax - TotalDiscount`       |

---

## 10. Aggregate Invariants

| ID   | Component         | Invariant                                                                                   |
|------|-------------------|---------------------------------------------------------------------------------------------|
| I-01 | `DocumentLine`    | `NumItem` must be strictly sequential (1, 2, 3...)                                          |
| I-02 | `FiscalDocument`  | Cannot exceed 2,000 line items (MH schema constraint for FE).                               |
| I-03 | `FiscalDocument`  | `DocumentTotals` must exactly equal the mathematical sum of all lines.                      |
| I-04 | `FiscalDocument`  | `DocumentTotals` must be `DocumentTotals.Zero` if the document has no lines.                |
| I-05 | `FiscalDocument`  | `AddLine` and `RemoveLine` fail if `Status` is not `Draft`.                                 |
| I-06 | `DocumentLine`    | `Description` length must be between 1 and 1500 characters.                                 |

---

## 11. Validation Rules

| Error Code                               | Condition                                |
|------------------------------------------|------------------------------------------|
| `Money.NegativeValue`                    | Value < 0                                |
| `Quantity.ZeroOrNegative`                | Value <= 0                               |
| `DocumentLine.DescriptionInvalidLength`  | Description < 1 or > 1500 chars          |
| `FiscalDocument.LineLimitExceeded`       | Attempting to add the 2001st line        |
| `FiscalDocument.LineNotFound`            | Attempting to remove a `NumItem` that does not exist |
| `FiscalDocument.InvalidStateForModification` | Adding/removing lines when not in Draft status |

---

## 12. Acceptance Criteria

- [ ] `Money` and `Quantity` Value Objects can be created and validate constraints.
- [ ] `DocumentLine` entity can be constructed through an internal factory.
- [ ] `FiscalDocument` can add lines via `AddLine(...)`.
- [ ] `NumItem` is automatically assigned based on line position.
- [ ] `FiscalDocument.Totals` automatically recalculates precisely when a line is added.
- [ ] `FiscalDocument` can remove lines via `RemoveLine(...)`.
- [ ] Removing a line correctly recalculates `Totals` and re-sequences the remaining `NumItem`s.
- [ ] Adding/removing lines fails if `Status != Draft`.
- [ ] Exceeding 2000 lines yields a `FiscalDocument.LineLimitExceeded` error.

---

## 13. Definition of Done

1. Value Objects (`Money`, `Quantity`, `DocumentTotals`) implemented and unit tested.
2. `DocumentLine` entity implemented.
3. `FiscalDocument` aggregate methods implemented.
4. Total calculation logic thoroughly unit tested for precision and correctness.
5. All tests pass (`dotnet test`).
6. Code complies with repository formatting standards.

---

## 14. Testing Requirements

- **Totals Recalculation:** Extensive test coverage verifying that adding/removing lines produces the exact correct `DocumentTotals`, particularly testing mixed lines (e.g., Line 1 is Taxable, Line 2 is Exempt).
- **Line Re-sequencing:** Verify that if Line 2 is removed from a 3-line document, Line 3 becomes Line 2, and `NumItem` remains contiguous.
- **Immutability of Totals:** Ensure there is no public setter on `DocumentTotals` within `FiscalDocument`.
- **State Enforcement:** Tests verifying `InvalidStateForModification` when attempting to add lines to an already transmitted/signed document.

---

## 15. Documentation Requirements

- Save this specification artifact to `docs/implementation/issue-008-fiscal-document-body.md`.
- No other documentation required for this design task.

---

## 16. Future Follow-up Issues

| Issue    | Title                                 | Dependency on #008                                           |
|----------|---------------------------------------|--------------------------------------------------------------|
| **#009** | Document-Type Validation Rules        | Will require checking if CCF has specific calculation limits compared to FE. |
| **#010** | DTE JSON Payload Generation           | Infrastructure layer needs the complete Aggregate to map to MH JSON structure. |

---

## Architecture Review Board Verdict

### Classification: **APPROVED**

### Justification
1. **Appropriate Aggregate Boundary:** The lines are clearly subordinate to the document and have no existence outside of it. `DocumentLine` correctly models a local entity.
2. **Invariant Protection:** By strictly coupling `DocumentTotals` generation to the addition/removal of lines, the aggregate guarantees that the `resumen` and `cuerpoDocumento` are never out of sync. This addresses a major risk in financial software design.
3. **Avoidance of Speculative Engines:** Pushing the responsibility of determining tax logic (pricing, exemptions, rules) to the Application layer prevents the Domain layer from becoming bloated with regulatory volatility. The domain only guarantees pure mathematical integrity.
4. **Valid DTE Composition:** The resulting model represents exactly what is needed for a valid FE/CCF document payload without superfluous ERP-like concepts.
