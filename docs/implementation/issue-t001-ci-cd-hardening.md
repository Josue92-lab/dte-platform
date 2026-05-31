# Issue T-001: CI/CD Hardening

**Status:** Completed  
**Date:** 2026-05-31  

## Objective
Upgrade the repository's initial validation scripts into an enterprise-grade engineering platform. This establishes secure dependency management, static security analysis, caching, coverage tracking, and pipeline governance.

## Existing Weaknesses Discovered
1. **Outdated Actions:** Workflows utilized `actions/checkout@v3` and `setup-dotnet@v3`, missing Node 20 runtime improvements and native caching features.
2. **Missing NuGet Caching:** Dependencies were restored freshly on every run, wasting network bandwidth and increasing execution latency.
3. **Architecture Test Sequence Risk:** The architecture test previously executed `dotnet build` exclusively on its own project, potentially ignoring full-solution compilation warnings or missing global dependency graphs.
4. **No Security Posture:** No static analysis (SAST) or dependency vulnerability monitoring existed.
5. **No Test Visibility:** Tests executed successfully but yielded no measurable coverage data for long-term health tracking.

## Improvements Implemented
- **GitHub Actions Modernization**: Upgraded all uses of `checkout` and `setup-dotnet` to `@v4` across all workflows.
- **Caching**: Leveraged `cache: true` natively within `setup-dotnet@v4` on all workflows to securely cache NuGet packages.
- **Architecture Validation**: Altered the sequence to run a global `dotnet build DtePlatform.sln` first, ensuring the holistic compilation passes before executing boundary validations. Added verbose `.trx` reporting published as artifacts on failure.
- **Test Coverage**: Integrated `--collect:"XPlat Code Coverage"` natively into `dotnet test` executions. Added `actions/upload-artifact@v4` to store `coverage.cobertura.xml` files.

## Security Benefits
- **CodeQL Integration**: A new `codeql.yml` workflow automatically scans C# code during PRs and on a weekly cadence to catch severe coding vulnerabilities (SAST).
- **Dependabot**: A new `dependabot.yml` configuration monitors NuGet ecosystem vulnerabilities and GitHub Actions drift, providing automated weekly PRs.

## Performance Benefits
- Utilizing native GitHub Actions cache mechanisms for NuGet reduces external network hits to `api.nuget.org`.
- Explicit `--no-restore` and `--no-build` flags in the test commands ensure the pipeline does not redundantly compile the solution across consecutive steps.

## Future Recommendations
- Configure formal Branch Protection Rules on the `main` branch to require passing `pr-validation.yml` and `CodeQL` checks.
- Introduce SonarQube or similar tooling to ingest the generated `cobertura` files and establish hard Quality Gates.
