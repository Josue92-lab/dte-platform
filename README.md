# DTE Platform (V1)

The single, legally compliant issuance engine for Factura Electrónica (01) and Comprobante de Crédito Fiscal (03).

## Prerequisites

- .NET 8 SDK
- Docker Desktop

## Local Development Bootstrap

1. Clone the repository.
2. Boot the database:
```bash
docker-compose -f deploy/docker-compose.dev.yml up -d
```
3. Restore dependencies:
```bash
dotnet restore DtePlatform.sln
```
4. Run the API:
```bash
dotnet run --project src/DTE.Api
```
5. Verify health:
Navigate to http://localhost:5001/health

Automated Tests
● Unit Tests: dotnet test tests/DTE.Domain.Tests
● Architecture Verification: dotnet test tests/DTE.Architecture.Tests
● Full Suite (Requires Docker): dotnet test DtePlatform.sln

---
## 17. Commit 0 Verification Checklist

Before accepting Commit 0, the Lead Engineer must verify:

- [ ] All files specified in this manifest exist in the exact directory structure.
- [ ] `dotnet build DtePlatform.sln` completes with 0 errors and 0 warnings.
- [ ] `dotnet test tests/DTE.Architecture.Tests` passes successfully.
- [ ] `docker-compose -f deploy/docker-compose.dev.yml up -d` successfully starts PostgreSQL on port 5432.
- [ ] The `.github/workflows` YAML files are valid.
- [ ] `global.json` locks the repository to .NET 8.0.x.

**Status:** Ready for Repository Generation.
