# P6 versioned email-template registry — 2026-10-30

Implementation package is ready for coordinator integration verification. No provider call, mail send, secret read/write, seed content, live database apply, commit or push was performed by this contributor.

## Contract and ownership

[Design](../design/email-templates-p6.md) records the agreed company-wide policy. Admin creates verified immutable plain-text versions and separately activates an exact version with expectedRevision. Admin/Manager can read active settings, bounded version pages and exact immutable detail. Empty state is revision1/active:null; no fabricated UUID or default text is installed.

Canonical module: `docs/sdd/contracts/crm/email-templates.openapi.yaml`. Operations are getCrmEmailTemplates, getCrmEmailTemplateVersion, getCrmActiveEmailTemplate, createCrmEmailTemplateVersion and activateCrmEmailTemplate. Aggregate paths are supplied to coordinator separately. Request DTOs reject unknown members; writes are Admin-only, actor-bound requestId receipts replay the same response and changed payload conflicts. Pagination preserves value/pagedInfo. Subject max200, body max3000, name max120; explicit placeholders recipientName/managerName/quoteNumber only. Subject header line breaks, null control characters, malformed or unknown placeholders are rejected.

Substitution is non-recursive, does not evaluate user template code, validates context values, applies final200/3000 output bounds, and HTML-encodes the result. Sending is outside this registry. Literal HTML in stored plain text never becomes executable HTML output.

## File-role map

```text
Crm.Domain/Documents/EmailTemplates/                  typed IDs, immutable version, active-state revision and validation
Crm.Application/Documents/EmailTemplates/             DTOs, service/store/reader ports and plain-text renderer
Crm.Infrastructure/Persistence/Documents/EmailTemplates/  read/write/mutation-journal partials and receipt record
Crm.Infrastructure/Persistence/Configurations/EmailTemplates/  version/state/journal mappings
Crm.Api/Documents/EmailTemplates/                     authorized controller and strict create/activate requests
```

IntroductionTemplateService requires the real IEmailTemplateVersionReader. New selection references and changed references must resolve an existing registry version. The exact unchanged reference on an accessible historical selection may be preserved, including unresolved local legacy IDs; this does not make it eligible for delivery. New issuer-document IDs are never substituted. Two existing integration fixture constructors were updated to pass the real EmailTemplateStore reader.

## Database

Additive CRM migration `20261030150013_AddEmailTemplateRegistry` adds EmailTemplateVersions (typed long PK, public UUID and unique sequence number), singleton EmailTemplateState/revision/FK and EmailTemplateMutations with actor/operation/requestId unique key. All writes share a bounded administrative registry advisory lock inside one transaction, including allocation, state pointer and replay outcome. Append-only UPDATE/DELETE trigger protects version rows. Existing production artifact migrations can be generated after this migration; the coordinator owns apply order.

There is no implicit seed/activation. Deploy this migration before enabling the new API/reference checks. Down removes registry/history and is destructive after use; use a coordinated verified backup/forward recovery under the project runbook. No contributor database apply was run.

## Tests and verification

- Initial Crm.Application, Crm.Infrastructure and Crm.Api scoped builds passed with0 warnings/errors.
- Initial Domain suite passed6/6. One later final-output-limit regression was added, so final coordinator unit filter is `FullyQualifiedName~EmailTemplateVersionTests` (7 cases).
- Contract/API metadata tests: `FullyQualifiedName~EmailTemplateRegistryContractTests` (2 cases, no PostgreSQL): strict request deserialization, write roles, five operations and page envelope. An initial integration compilation exposed relative namespace qualification in two legacy fixtures; both were corrected to `global::Crm.Infrastructure...` before the coordinator final run. Do not count that failed compile as a passing contract run.
- PostgreSQL filter: `FullyQualifiedName~EmailTemplateRegistryPersistenceTests` (3 cases): roles, create/activate replay and conflicts, active revision, immutable history, bounded page/count, SQL append-only triggers, valid/new/missing/unchanged legacy introduction references and foreign-owner denial. The fixture uses only uniquely named isolated databases. Final execution is the coordinator's safe local wrapper, without printing connection strings.
- Scoped `git diff --check` on package source passed before the final namespace-only test correction; coordinator final diff check remains authoritative.

Contributor stopped additional builds to avoid shared-build contention and handed frozen files to the coordinator. Final PostgreSQL/contract/expanded-unit results must be appended from that unified run before claiming complete verification. Frontend administration/selection integration and provider/outbox sending remain separate slices.


Coordinator results and actual local database apply are authoritative in [unified checkpoint](p6-backend-checkpoint-2026-10-30.md).
