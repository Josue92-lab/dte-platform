# Implementation Impact Map

This map outlines how specific MH artifacts impact the internal layers of the DTE Platform codebase.

## Domain Layer Impact
**Artifacts:** `Normativa de Cumplimiento...v2.0.pdf`, `Catálogos - Facturación Electrónica`
- **Affected Aggregates:** `FiscalDocument`, `ControlNumberSeries`, `Counterparty`, `Item`.
- **Affected Value Objects:** `ControlNumber` (format), `Money` (rounding rules), `DteType` (catalog), `TaxIdentifier` (validation rules).
- **Future Issues:** Implementing calculation rules for `TaxSummary`, implementing `Counterparty` validation thresholds (FE vs CCF limits).

## Application Layer Impact
**Artifacts:** `Manual Funcional del Sistema de Transmisión V 2.0.pdf`, `svfe-json-schemas/`
- **Affected Workflows:** Document signing orchestration, Transmission attempts, Invalidation requests, Contingency episode management.
- **Affected DTOs:** Mapping Domain Aggregates to the exact JSON schema versions required by `svfe-json-schemas/`.
- **Future Issues:** Building the `SignDocumentCommand`, `SubmitDocumentCommand`, and the `ContingencyReconciliationSaga`.

## Infrastructure Layer Impact
**Artifacts:** `Manual Técnico para la Integración...v2.pdf`, `svfe-api-firmador/`
- **Affected Integrations:** `MhTransmissionClient`, `MhAuthenticationClient`.
- **Affected Security:** `DteSigner` service (must match the Java `svfe-api-firmador` output exactly).
- **Future Issues:** Implementing the OAuth token rotation for MH APIs, implementing resilient HTTP retries for transmission, implementing the PKCS#8 / PKCS#12 signature algorithms.
