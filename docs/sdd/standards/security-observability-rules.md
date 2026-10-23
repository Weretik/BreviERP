# Security and observability rules

## Security and privacy

Follow the principle of least privilege. The host fallback policy requires authentication; mark intentionally public endpoints with `AllowAnonymous`. Preserve existing role and policy rules. See [Identity and access controls](../architecture/security/README.md) for model details.

Do not log tokens, passwords, connection strings, PII, or confidential request data. Do not return internal exception details in production responses.

## Logs and diagnostics

Use structured Serilog messages with stable, useful properties. Preserve correlation, cancellation, and error diagnostics. Log at an appropriate level without noise.

Handle transient failures explicitly and with an observable result. Do not introduce implicit retries for writes that can duplicate side effects.
