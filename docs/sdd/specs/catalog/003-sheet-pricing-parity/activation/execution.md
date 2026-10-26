# TS-008D — authorized local confirmation and publication

Specified before implementation, 2026-10-26. The human explicitly confirmed using the reviewed 2026-10-25 source locally, confirming inputs and publishing Sewing prices. DR-C01 retains the 4-minute operation under ordinary backend rules. Persistent exclusions: SKU 724 stays blocked with missing supplier data; SKU 931 stays inactive. These decisions remain until a later explicit change.

## Rules and acceptance

- R-D01 / SC-D01: create an ignored immutable activation plan from a fresh clean source review and current identities/versions, retaining semantic composition, rule/offer values, settings/reference IDs, exclusion list and stable request UUIDs. Plan creation writes no business data. Existing numeric ID types remain; plan/request UUIDs are operational identities, not new entity keys.
- R-D02 / SC-D02: apply only to a verified loopback database whose name matches the plan, with explicit confirmation, exact plan SHA-256 and a validated pg_dump custom-format backup. Validate the existing local Admin/Manager account and lockout state in Host.Seed before writes. Attribute records to its actual numeric user ID; do not create users/roles, impersonate an HTTP session or fabricate an approval identity. This trusted local operator command is explicitly authorized in this task, not a remote unauthenticated API.
- R-D03 / SC-D03: confirm common and Article-specific composition through existing ReplaceSewingComposition/ReplaceSewingArticleComposition handlers with expected versions. Preserve every number, reference, scope and manual operation; set review metadata through the Domain factory. Settings/Reference versions are already approved and are not rewritten. PPE confirmations call existing CreateVersion handlers with expected version and stable UUIDs. Preserve original imported drafts.
- R-D04 / SC-D04: save/publish through existing Application handlers. Require a strict calculable result and the planned settings/reference IDs; compare saved calculated tiers to the fresh plan calculation, never inject price values. Existing stale-input checks/publication history remain authoritative. 452/453 have no special runtime formula/price override.
- R-D05 / SC-D05: SKU 724 and inactive 931 must never be confirmed, calculated as working PPE or reactivated by this plan. Their original draft IDs/approval state remain unchanged. All 248 Sewing Articles and 117 eligible PPE Products must have current prices after completion.
- R-D06 / SC-D06: execution is an ordered sequence of existing atomic workflows, not one catalogue-wide transaction. Persist a local progress report after each resource. Resume only unchanged semantic inputs and recognized completed review scopes; reject stale composition versions/changed values before overwriting. Request UUID replay and publication idempotency prevent duplicate history. Failures preserve committed steps for explicit continuation, never silently roll back history.

## Responsibility map

Add `Jobs/SewingPricing/Activation/` with plan/report records per file, plan builder, composition/PPE confirmation, existing-handler price publication and guarded orchestration. Job adapter `Reading/JobProductReferenceReader` supplies real Reference data for existing composition validation. Host.Seed owns actor validation, backup/plan checks and explicit plan/apply modes; runtime modules do not depend on jobs. No migration, schema, new formula, HTTP API or frontend contract change is required.

Original import line mappings remain immutable provenance. Existing Replace handlers recreate line identities; source review must resolve a replacement by unique semantic scope/reference/order when the original mapped line no longer exists. Values still undergo exact comparison, missing/extra/changed lines still fail. Add regression covering source review after legitimate confirmation.

## Execution and recovery

1. Fresh review/plan, actor and exclusions validated; persist plan/hash.
2. Create pg_dump backup of the exact configured local database; verify pg_restore listing, header, size and SHA-256. Keep secrets out of output and arguments.
3. Confirm common then Article composition per model; append reviewed PPE rules then eligible offers. Keep raw quantities/source values unchanged.
4. Strict calculation, save, publish per Article; record saved/publication IDs. Reopen through current published-price/PPE read ports and assert exclusions.
5. Rerun full readiness/source review and check 248 published Sewing + 117 calculable PPE, with only 724/931 held. Record exact evidence and retained local backup/report paths.

Before confirmation retain plan scope/versions and backup. Prefer append-only corrections and new saved/publication versions after a later business decision; never delete history or rewrite global formulas to restore cached outputs. Full backup restoration is disaster recovery only, with explicit approval and an offline reviewed target. Do not auto-restore it on a partial failure. No production deployment/commit is authorized.

Focused verification: unit guard/semantic checks, isolated PostgreSQL apply and rerun, stale-plan rejection, excluded offer preservation, source-review-after-replacement and common pricing/publication behavior. Run relevant existing history/contract tests, not the full suite. [Execution evidence](execution-evidence.md).
