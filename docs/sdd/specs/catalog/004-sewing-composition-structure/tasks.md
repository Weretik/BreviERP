# Remediation tasks

| ID | Depends on | Work and evidence target |
| --- | --- | --- |
| RM-001 | — | **Done.** Split Application requests, handlers, validators, and response DTOs into matching files and use-case directories. Evidence: 34 focused unit tests and 19 integration tests passed. |
| RM-002 | — | **Done.** Keep readiness rules on Product and make projection adapter return read data only. Evidence: focused unit and API/query integration checks passed, preserving blocker mapping. |
| RM-003 | — | **Done.** Split tracked aggregate writes and no-tracking read projections into separate persistence adapters and interfaces. Evidence: focused persistence/projection checks passed. |
| RM-004 | — | **Done.** Split sewing enum types from the material entity file and reference ID types into matching files. Evidence: unit, integration, architecture checks and Host API build passed; no migration/schema change. |
| RM-005 | RM-001–RM-004 | **Done.** Verify architecture and formatting; update SDD002 audit, feature map and data model with actual final paths; preserve existing API behavior. Evidence: focused checks recorded in `README.md`. |
| RM-006 | RM-001 | **Done.** Separate `SewingArticlesController` (list/create) from `SewingCompositionController` (read/shared replace/Article replace). Preserve route names, authorization, binding and response shapes. Evidence: final focused Sewing API tests passed, 5/5. |
