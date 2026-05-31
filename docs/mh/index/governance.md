# MH Document Governance

This document establishes the governance model for maintaining the regulatory knowledge system of the DTE Platform.

## 1. Adding New MH Documents
1. Any new release from the Ministerio de Hacienda (MH) must be placed in the appropriate subfolder within `docs/mh/` (e.g., `catalogs/`, `manuals/`, `schemas/`).
2. The `mh-document-catalog.md` must be updated immediately with the new file, categorizing its purpose and importance.
3. If the new document supersedes an existing one, the old document must be moved to a `docs/mh/archive/` folder, and the new one takes its place.
4. The `authoritative-sources.md` must be updated to classify the new artifact.

## 2. Maintaining Traceability
1. Whenever an `AUTHORITATIVE` document is updated (e.g., a new schema version or catalog update), a corresponding Implementation Issue must be opened.
2. The `regulatory-traceability.md` and `implementation-impact-map.md` files must be reviewed quarterly or upon any major MH release to ensure they accurately reflect the current platform dependencies.

## 3. Referencing MH Artifacts in Future Issues
All future Implementation Issues generated for the DTE Platform MUST cite the specific `AUTHORITATIVE` MH artifact that demands the change.
- **Example:** "Implement CCF validation rules as defined in *Normativa de Cumplimiento V2.0*, Section 4.1."
- **Example:** "Update Unit of Measure value objects based on *Catálogos - Facturación Electrónica* update 2026-X."

*No business logic may be invented without a direct citation to an Authoritative artifact.*
