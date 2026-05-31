# MH Document Catalog

This document provides a complete inventory of the Ministerio de Hacienda (MH) regulatory artifacts present in this repository.

| File Name | Document Type | Purpose | Regulatory Area | Recommended Category | Importance Level | Notes |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `Aviso mandatario referente a ventas con factura.pdf` | Legal Notice | Mandatory notification on invoice sales. | Legal/Compliance | `annexes` | Medium | Important for specific business rules regarding billing limits. |
| `Catálogos - Facturación Electrónica.xlsx` / `.pdf` | Catalog | Defines all enumerated values, codes, and static master data. | Data Model | `catalogs` | Critical | Directly drives the `ValueObject` implementations and domain validations. |
| `Guía del Proceso de Incorporación para ser Emisor...pdf` | Guide | Administrative process to become an issuer. | Administrative | `manuals` | Low (for code) | Process documentation; does not affect domain logic. |
| `Manual Funcional del Sistema de Transmisión V 2.0.pdf` | Manual | Functional flow of transmission, invalidation, and contingency. | Workflow | `manuals` | High | Defines the state machine rules for the transmission lifecycle. |
| `Manual Técnico para la Integración Tecnológica...v2.pdf` | Technical Spec | Technical API integration, endpoints, auth, and payloads. | Integration | `manuals` | Critical | Directly dictates Infrastructure layer API client implementations. |
| `Manual de Usuario de Consulta Pública...pdf` | Guide | Public portal usage. | Administrative | `manuals` | Low | Out of scope for core issuance platform. |
| `Manual de Usuario del Sitio de Emisores DTE.pdf` | Guide | Issuer portal usage. | Administrative | `manuals` | Low | Out of scope for core issuance platform. |
| `Manual de acreditamiento y obtención de certificado...pdf`| Guide | Process for acquiring the signing certificate. | Security | `manuals` | Medium | Affects the configuration and key management infrastructure. |
| `Manual del Usuario para la Solicitud de Ingreso...pruebas.pdf`| Guide | Test environment onboarding. | Administrative | `manuals` | Low | Important for QA, not for code. |
| `Manual para la obtención de la Autorización...pdf` | Guide | Final authorization process. | Administrative | `manuals` | Low | Process documentation. |
| `Normativa de Cumplimiento de los DTE_Versión 2.0.pdf` | Regulation | The supreme regulatory rules for DTE structural compliance. | Domain | `validation-rules` | Critical | The ultimate source of truth for invariants and business rules. |
| `svfe-api-firmador/` | Source Code | Reference implementation for the cryptographic signer. | Security | `examples` | High | Used to validate our `.NET` implementation of the firmador. |
| `svfe-json-schemas/` | Schema | JSON schema definitions for all DTE types and events. | Data Model | `schemas` | Critical | Strict format enforcement for payload generation. |
