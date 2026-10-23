# Background job CLI

A separate `Host.Jobs` CLI or set of job commands is not implemented in current BreviERP. Do not use this document as a runbook with invented `dotnet run` parameters.

If such a host is added, its documentation must record:

- the exact project path and how configuration is loaded;
- every job name, required argument, and exit code;
- separate secrets for local execution;
- concurrency, retry, and idempotency rules;
- verification of the result after completion.

Until it exists, use only the verified `Host.Seed` for initial data operations.
