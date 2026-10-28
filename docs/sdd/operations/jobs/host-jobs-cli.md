# Background job CLI

A separate `Host.Jobs` CLI is not implemented. Existing operational jobs from `src/Jobs` run through `Host.Seed` flags; their exact commands are in the runbooks listed in [Background jobs and runbooks](README.md). Do not invent `dotnet run` parameters.

If a separate jobs host is added, its documentation must record:

- the exact project path and how configuration is loaded;
- every job name, required argument, and exit code;
- separate secrets for local execution;
- concurrency, retry, and idempotency rules;
- verification of the result after completion.

Until it exists, use only the verified `Host.Seed` commands.
