# CI/CD Governance

This document outlines the pipeline governance strategy for the DTE Platform, establishing enterprise-grade rules for continuous integration, delivery, and security.

## Purpose of Workflows

1. **PR Validation (`pr-validation.yml`)**: Executes automatically on Pull Requests targeting `main`. Responsible for enforcing code formatting (`.editorconfig`), architecture rules, treating warnings as errors, and executing the full test suite with coverage. This ensures code cannot be merged unless it proves to be stable and compliant.
2. **CI Build & Test (`ci.yml`)**: Executes on pushes to `main`. Acts as the definitive build of record. Runs tests per layer incrementally and collects coverage artifacts. This will be the precursor for future CD deployment pipelines.
3. **Architecture Enforcement (`architecture-validation.yml`)**: Executes on all branches. Focuses exclusively on protecting the domain and layer boundary invariants. Builds the entire solution first to ensure dependency graphs are respected, then runs `DTE.Architecture.Tests`.
4. **CodeQL Analysis (`codeql.yml`)**: Scheduled and PR-triggered static application security testing (SAST). Scans the C# codebase for known vulnerabilities and secrets.

## Execution Triggers

- **Push (`main`)**: Triggers `ci.yml`, `codeql.yml` (and `architecture-validation.yml`).
- **Pull Request (`main`)**: Triggers `pr-validation.yml` and `codeql.yml`.
- **Scheduled**: CodeQL runs weekly (Sunday 02:30). Dependabot runs weekly (Monday) to propose NuGet/Actions updates.

## Ownership and Governance

- **Maintainers**: Platform Engineering owns the `.github/` configurations.
- **Developers**: Must respect PR validations. No force-merging allowed if architecture or security scans fail.

## Security Responsibilities

- **CodeQL**: Provides SAST capabilities. Critical and High vulnerabilities must fail the PR workflow.
- **Dependabot**: Identifies vulnerable transient and direct NuGet packages and out-of-date GitHub Actions. It is configured to generate weekly, low-noise update PRs using conventional commits (`build(deps)`).

## Failure Handling

- **Architecture Tests**: Any violation of namespace boundaries immediately fails the build, preventing architectural erosion. Detailed `trx` logs are output as artifacts.
- **Test Failures**: Break the build.
- **Future Enhancements**: We do not yet enforce hard coverage thresholds, but we collect `coverage.cobertura.xml` artifacts. A future policy will establish Quality Gates using these outputs.

## Future Expansion Guidelines

- When Continuous Delivery (CD) is introduced, it should trigger only upon the successful completion of `ci.yml` on the `main` branch.
- Add load testing / integration testing workflows in dedicated staging pipelines.
