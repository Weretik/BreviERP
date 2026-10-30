# P6 versioned email template registry

Approved implementation contract: company-wide immutable email template versions with typed long relational IDs and UUID PublicId. Admin creates verified plain-text name/subject/body versions and separately activates an exact version with expectedRevision. Admin/Manager can read active state, bounded version summaries and exact historical detail. No template content is seeded or copied from issuer documents or legacy credentials.

Subject max200, body max3000 and name max120. Only recipientName/managerName/quoteNumber placeholders are supported; malformed/unknown braces and subject line breaks are rejected. Substitution is non-recursive and HTML output is escaped. Creation/activation use actor-bound requestId journal and serial transaction locks. Active-state revision starts1; absence is represented as revision1/active:null. Migration adds separate CRM tables and append-only version trigger; rollback after real use is coordinated backup/forward repair, not destructive Down.

API namespace `/api/crm/email-templates`: GET list, POST `/versions`, GET `/{versionId}`, GET/PUT `/active`. Generic Result mapping yields400/403/404/409. Canonical OpenAPI is `docs/sdd/contracts/crm/email-templates.openapi.yaml`. No sender, mail queue or provider call is introduced here.

New introduction template references must resolve an existing immutable email version. Revising an accessible existing selection may preserve its unchanged historical reference even if unresolved, allowing historical local data to remain editable; this does not make it eligible for sending. A changed/new reference must resolve; no issuer UUID substitution.
