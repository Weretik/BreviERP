# Background job troubleshooting

A runbook for an implemented job should contain checks that can be performed without exposing sensitive data.

| Symptom | First checks | Do not |
| --- | --- | --- |
| Missing configuration | Environment and approved secret store | Put connection strings or credentials in logs |
| External service authentication failure | Secrets and endpoint reachability | Log headers, passwords, or Authorization values |
| Zero records received | Filter, parameters, environment, source availability | Manually clear data or repeat destructive imports |
| Records received but not saved | Mapper, validation, constraints, and error logs | Rerun while data is changing |
| DLL locked during build | Stop local Host.Api and retry | Delete the running host's output |

Record the job name, environment, safe identifiers, duration, counters, exception type, and database migration version. This supports diagnosis without storing full payloads or secrets.
