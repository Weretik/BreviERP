# EN-004 Gmail provider readiness checkpoint — 2026-10-30

**Status: partial implementation, provider disabled.** User selected Gmail/Google Workspace and supplied sender/test-recipient choices to the coordinator. The later correction uses a distinct test recipient. Live addresses are deliberately not reproduced in tracked documentation or fixtures. No OAuth credential exists in this implementation, no authorization flow was executed, no message was sent, and no provider-status round-trip was claimed.

## Existing mechanisms inspected

Host.Api uses WebApplication.CreateBuilder and the existing host configuration; Identity only has user email/auth fields, not a mail sender. A keys-only presence scan found zero SMTP/Gmail/delivery provider keys in Host.Api appsettings/development, Host.Seed appsettings, the Host.Api user-secrets source, and relevant environment names. No secret values or legacy Calculator Gmail tokens were printed, copied or imported.

## Implemented boundaries

- Application `Delivery/Readiness/`: IQuoteDeliveryReadiness, safe blocker-only result and trusted adapter capabilities.
- Infrastructure `Delivery/Readiness/`: non-secret QuoteDeliveryProviderOptions, pure recipient-specific readiness policy and explicit unavailable token source.
- Application `Delivery/Provider/`: pinned public submission, access-token port and confirmed-success/confirmed-failure/unknown result. Outbox owns mutation/replay and first-attempt permission; this adapter cannot promise exactly-once sending.
- Infrastructure `Delivery/Gmail/`: one-attempt fixed-endpoint Gmail transport, stable v1 Message-ID correlation using the configured sender domain, MIME/plaintext/PDF encoding and bounded JSON parser. Reconciliation searches Sent by RFC Message-ID, then verifies exact metadata identity and SENT label. Empty, ambiguous, malformed or unavailable lookup remains Unknown; reconciliation never calls send.

Readiness defaults: Enabled=false, SandboxOnly=true. It rejects missing/mismatched adapters, absent credentials, unsupported stable identity/status lookup, invalid/header-injected addresses, customer recipients in sandbox, unbounded timeouts or a lease too close to the timeout. Timeout range2–60 seconds; lease30–300 with at least15-second headroom. No credentials are stored in options/results and blocker codes contain no addresses.

Proposed host keys: `Crm:Delivery:{Enabled,ProviderKey,SenderAddress,SandboxOnly,TestRecipientAddress,TimeoutSeconds,LeaseSeconds}`. ProviderKey is `gmail` after the user selection, but Enabled must remain false until actual OAuth readiness is proven. Delegated OAuth source provisioning requires secure configuration such as `Crm:Delivery:Gmail:OAuth:{ClientId,ClientSecret,RefreshToken}`; these are proposed names, not configured values. Register a plain typed HttpClient with no automatic retry policy. Use UnavailableQuoteDeliveryAccessTokenSource until the reviewed OAuth source exists. Freeze provider and sender identity with every outbox delivery before a future change of account/domain.

## Official-provider fit and gaps

Gmail send uses a base64url RFC2822 MIME message and returns a message resource. The documented request has no idempotency-key parameter; therefore the v1 Message-ID is correlation only, not Gmail server-side deduplication. A positive response confirms provider acceptance, not recipient delivery/read. [Google users.messages.send](https://developers.google.com/workspace/gmail/api/reference/rest/v1/users.messages/send), [sending guide](https://developers.google.com/workspace/gmail/api/guides/sending).

Gmail list supports `rfc822msgid:` queries and the SENT label; metadata scope alone cannot use q. Sending plus lookup needs `gmail.send` and `gmail.readonly` in this design. List returns identifiers, so the adapter fetches metadata and checks the expected Message-ID rather than assuming the search match proves success. An empty search is not proof of non-delivery and cannot unlock resend. [Google messages.list](https://developers.google.com/workspace/gmail/api/reference/rest/v1/users.messages/list), [messages.get](https://developers.google.com/workspace/gmail/api/reference/rest/v1/users.messages/get).

OAuth must use the confirmed account's delegated consent/offline access; the source is not implemented by this checkpoint. Send is sensitive and read-only is restricted, so appropriate OAuth consent/verification must be resolved before enabling. No broader modify/delete scope is needed. [OAuth web-server flow](https://developers.google.com/identity/protocols/oauth2/web-server), [Gmail scope reference](https://developers.google.com/workspace/gmail/api/auth/scopes).

## Verification

Readiness tests run in IntegrationTests because UnitTests has no Infrastructure reference; they need no PostgreSQL. Initial mistaken UnitTests placement failed compilation and was corrected (not counted as Red). Compilable non-sending stub produced Red10 failed,0 passed. Implemented policy produced Green10/10 passed. Crm.Infrastructure build passed with0 warnings/errors after adapter MIME quoting correction. Fake HTTP Gmail tests cover MIME/fixed endpoint/correlation, unknown response and timeout without retry, empty lookup without send, exact metadata matching, sandbox/OAuth/disabled gates, header injection and malformed success JSON. The contributor invocation stalled during concurrent builds and was cancelled without a result; the coordinator must run the combined filter below on the final source state.

```powershell
dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter 'FullyQualifiedName~QuoteDeliveryReadinessPolicyTests|FullyQualifiedName~GmailQuoteDeliveryProviderTests' --no-restore --nologo --verbosity quiet
```

Scoped diff check passed. No shared DI/API/schema/migration was changed by this package. No commit or push executed. EN-004 completion still requires actual delegated OAuth configuration, sender/account proof, controlled recipient authorization and a confirmed provider round-trip/status-lookup test; current fake responses do not satisfy those operational gates.


Coordinator results and actual local database apply are authoritative in [unified checkpoint](p6-backend-checkpoint-2026-10-30.md).
