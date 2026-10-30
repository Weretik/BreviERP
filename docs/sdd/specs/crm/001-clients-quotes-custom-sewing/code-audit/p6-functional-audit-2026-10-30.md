# P6 functional and responsibility audit — 2026-10-30

## Scope and method

User requested an audit and responsibility split of the just-delivered P6 work. Reviewed the email registry, production projection/source/ZIP/persistence/API, Gmail MIME/send/reconciliation/readiness and composition root. Previous P5 rendering changes and unfinished P6 frontend/outbox are outside this patch. One agent performed the audit and fixes; no new delegation, Git commit or remote change.

Reviewed `AGENTS.md`, backend/delivery/testing standards, the current feature map, P6 contracts/designs and implementation. Existing additive migrations remain unchanged and are not reapplied for this audit. Public routes, operation IDs, payloads and database model remain unchanged. Existing immutable append-only constraints, exact issued-version selection and current-owner authorization were retained.

## Findings

| ID | Severity / status | Evidence and impact | Owner and completed change | Verification |
| --- | --- | --- | --- | --- |
| AF-P6-001 | P2 / fixed | `src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/Delivery/Gmail/GmailQuoteDeliveryProvider.cs`: response stream IO interruption escaped from send and reconciliation after HTTP headers. A provider outcome could be unknown while callers received an exception. | Gmail adapter: catch `IOException` at the external response boundary and preserve `Unknown`; do not resend. Dependencies: none. | `GmailQuoteDeliveryProviderTests.Interrupted_response_body_is_unknown_without_retry(false/true)` failed twice with the exact stream `IOException` before the fix; final regression result below. |
| AF-P6-002 | P3 / refactored | Same provider contained both the write operation and read-only Sent search/metadata verification. Future retry/lookup edits had two distinct flows in one type. | Extracted `Delivery/Gmail/GmailSentMessageLookup.cs`; public adapter keeps submit and delegates reconciliation. Existing MIME, bounded JSON and Message-ID helpers retain their separate roles. Dependencies: AF-P6-001 behavior retained in both operations. | Existing fake HTTP scenarios and the two interruption regressions; exact method/count assertions prove reconciliation never submits and no failure path retries. |
| AF-P6-003 | P3 / refactored | `DependencyInjection/CrmP6ServiceExtensions.cs` grouped independent registry, production and Gmail registrations under a temporary phase name. | Replaced with `CrmEmailTemplateServiceExtensions.cs`, `CrmProductionPackageServiceExtensions.cs`, `CrmQuoteDeliveryServiceExtensions.cs`; main `CrmInfrastructureExtensions.cs` explicitly composes all three. No dependency/interface change. | Infrastructure compiles as part of the focused integration build; registry/export/provider scenarios and CRM dependency check. |

No unresolved correctness blocker was identified in this bounded audit. This is scoped evidence, not a claim that unimplemented P6 sending/UI work is complete.

## Responsibilities retained

- Domain email-template files own immutable validation, placeholder syntax and active-version revision. Application owns text substitution/reference use cases. Registry persistence separates reads, writes and mutation transaction/replay; API binds and authorizes.
- Production Application separates versioned manifest contracts, selected-variant projection, saved operation basis and file allowlist, export/read ports and use cases. Persistence separates exact source loading, export transaction/journal, authorized historical reads and stored records. ZIP composition performs private byte/path/MIME/length/hash verification. No current master prices or full private basis JSON enters the manifest.
- Request replay verifies current access before comparing payload hashes. Issued-version FK, selection uniqueness, advisory/row locks and append-only triggers protect stored artifacts and receipts. Archived new export requests conflict; historical reads/replay retain current ACL checks. Failure before commit leaves no partial artifact/receipt.
- Gmail readiness is pure; MIME formatting, bounded JSON parsing and stable identity remain dedicated files. No token or real address is added to source/tests. OAuth remains unavailable, and sending remains disabled.

No further folder split was justified: the two domain areas already have separate feature directories; remaining production files have one primary role and are at most roughly106 lines, excluding generated EF migrations. A small cohesive use-case/file is not split solely to reduce line count. The metadata reader currently verifies stored byte integrity, so a metadata call may load/hash up to the bounded64MiB artifact; no throughput/performance benchmark was run.

## Final structure

```text
Crm.Infrastructure/Crm.Infrastructure/
  DependencyInjection/
    CrmInfrastructureExtensions.cs              module composition
    CrmEmailTemplateServiceExtensions.cs        registry registrations
    CrmProductionPackageServiceExtensions.cs    production export registrations
    CrmQuoteDeliveryServiceExtensions.cs        Gmail/readiness registrations
  Delivery/Gmail/
    GmailQuoteDeliveryProvider.cs               one-attempt submit and public adapter
    GmailSentMessageLookup.cs                   read-only reconciliation
    GmailMimeMessage.cs                        MIME/text/PDF encoding
    GmailResponseJson.cs                       bounded provider response parsing
    GmailMessageIdentity.cs                    correlation identity
  Delivery/Readiness/                          non-secret options and fail-closed policy
  Persistence/Documents/EmailTemplates/         registry reads/writes/replay
  Persistence/Documents/Production/            pinned source/export/read/journal
  Documents/Production/                        deterministic bounded ZIP
```

## Verification

Red: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~Interrupted_response_body_is_unknown_without_retry --no-restore --nologo --verbosity quiet` exited1: **2 failed**, both `System.IO.IOException` from `GmailResponseJson.ReadAsync`. A no-build normal-verbosity run confirmed the exact stacks through SubmitAsync and ReconcileAsync; it was diagnostic, not extra test coverage.

Final results are appended after the unified run. The run uses [the same scoped PostgreSQL/filter procedure](../evidence/p6-backend-checkpoint-2026-10-30.md), with two additional Gmail cases. It creates/drops unique test databases and does not modify application data. No live email, OAuth round-trip, frontend integration or load test is included.

Green/regression: safe P6 wrapper integration filter exited0, **44 passed, 0 failed, 0 skipped**, duration28seconds. This includes both new interruption cases and all previous registry/production/readiness/reference regressions. CRM dependency architecture filter with --no-build --no-restore exited0, **1 passed**. git -c core.safecrlf=false diff --check exited0. Old phase registration references are absent. Final Gmail provider57lines; Sent lookup66lines. No schema/API change, no migration or live send during this audit. Unit rules were unchanged; their prior13-case evidence remains in the checkpoint and was not redundantly rerun.
