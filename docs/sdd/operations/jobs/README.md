# Background jobs and runbooks

In the currently verified BreviERP structure, there is no separate console import host, internal scheduler, queue, delayed retry mechanism, distributed lock, or periodic execution. `Host.Api` calls `AddBackgroundJobRegistrations()`, but that method currently registers no services. Commands or procedures for jobs not yet implemented are undefined.

```text
operator / cron / CI → agreed job implementation → scoped module service → PostgreSQL / external adapter
```

- [Job CLI status](host-jobs-cli.md)
- [Troubleshooting](troubleshooting.md)

For a new background operation, create an SDD first. It must define the start command, contract, idempotency, timeout and retry behavior, locking, observability, safe rerun, and rollback. Never put secrets or real endpoint URLs in output, tracker tasks, or documentation.
