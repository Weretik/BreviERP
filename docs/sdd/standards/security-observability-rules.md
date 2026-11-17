# Security and observability rules

## Security and privacy

Follow the principle of least privilege. The host fallback policy requires authentication; mark intentionally public endpoints with `AllowAnonymous`. Preserve existing role and policy rules. See [Identity and access controls](../architecture/security/README.md) for model details.

Do not log tokens, passwords, connection strings, PII, or confidential request data. Do not return internal exception details in production responses.

## Package vulnerabilities

- CI (`verify` job in `.github/workflows/deploy-cloudrun.yml`) runs `dotnet list package --vulnerable --include-transitive` after restore; a High or Critical advisory in any direct or transitive NuGet package fails the build and so blocks the deploy. Fix it by updating the package (or pinning the transitive one in `Directory.Packages.props`), never by skipping the step.
- Dependabot (`.github/dependabot.yml`) opens one grouped pull request a month for NuGet minor and patch updates and one for GitHub Actions; security updates arrive on their own. They go through the same CI and review as any change.

## Logs and diagnostics

Use structured Serilog messages with stable, useful properties. Preserve correlation, cancellation, and error diagnostics. Log at an appropriate level without noise.

Handle transient failures explicitly and with an observable result. Do not introduce implicit retries for writes that can duplicate side effects.
