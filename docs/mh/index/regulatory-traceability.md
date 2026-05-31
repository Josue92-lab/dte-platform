# Regulatory Traceability Matrix

This matrix maps authoritative MH artifacts to their specific regulatory responsibilities within the DTE Platform.

## 1. DTE JSON Schemas (`svfe-json-schemas/`)
- **Governs:** The exact structural payload serialized and transmitted to MH. Defines required fields, data types, and nesting for FE (`01`), CCF (`03`), Invalidations, and Contingency events.
- **Depends On:** Application Layer (DTO serialization), Infrastructure Layer (Payload builders).

## 2. MH Catalogs (`Catálogos - Facturación Electrónica`)
- **Governs:** All enumerated values permitted within the system. This includes DTE Types, Tax Identifiers (NIT, DUI), Municipalities, Departments, Tax behaviors (IVA 13%, Exempt), and Unit of Measure codes.
- **Depends On:** Domain Layer (Value Objects constraints), Application Layer (Validation pipelines).

## 3. Normativa de Cumplimiento (`Normativa de Cumplimiento de los DTE_Versión 2.0.pdf`)
- **Governs:** The overarching business logic, invariants, and legal constraints. E.g., calculation rounding rules, required identity thresholds for FE vs. CCF, and contingency event windows.
- **Depends On:** Domain Layer (Aggregate Root invariants, Domain Services).

## 4. Manual Técnico y Funcional de Transmisión (`Manual Técnico...v2.pdf`, `Manual Funcional...V 2.0.pdf`)
- **Governs:** The integration choreography. Specifies how to authenticate, how to transmit a signed payload, how to interpret MH response codes (Sello de Recepción, Rechazo), and the workflows for invalidation and contingency transmission.
- **Depends On:** Infrastructure Layer (HTTP Clients, Resilience policies), Application Layer (Transmission state machine orchestration).

## 5. Firmador Reference Implementation (`svfe-api-firmador/`)
- **Governs:** The precise cryptographic algorithms, hashing (SHA-256), and canonicalization steps required to produce a legally binding digital signature over the JSON payload.
- **Depends On:** Infrastructure Layer (Security/Cryptography services).
