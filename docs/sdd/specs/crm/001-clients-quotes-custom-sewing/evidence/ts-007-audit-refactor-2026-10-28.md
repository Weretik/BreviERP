# TS-007 functional audit and cleanup decomposition — 2026-10-28

Scope: latest TS-007 implementation/shared integration. User requested local audit and restructuring. Earlier P4/quote work is preserved. No new task, commit/push, dependency, working-database action or cleanup worker.

**AF-CRM-P4-021 — Low / verified fixed.** Owner TS-007. `Crm.Infrastructure/Files/Cleanup/CustomTemporaryFileCleanup.cs:7` mixed byte-deletion orchestration with candidate SQL, reference checks and state-transition transactions. `TemporaryFileCleanupStore.cs` now owns short claim/complete transactions, SKIP LOCKED and fresh under-lock reference guards. `TemporaryFileCleanupClaim.cs` replaces the identity/provenance tuple with a typed immutable result. The public service keeps the bounded loop, cancellation and byte deletion outside DB transactions. The same DbContext, lock order, state transitions and retry behavior are preserved.

Keep FileSystemPrivateFileStore together: reserved publication, integrity reads, known-object deletion and scope quota locking implement one private-object storage protocol; ACL/path policy is already separate. Upload/finalize/read/verification/version scenarios and strict wire requests have their own folders/files. No extra project/module or generic repository abstraction is justified by this scope.

Review covered actual bytes/hash/MIME/size, quota, reserved expiry/crash paths, current/exact-quote version ACL, immutable flags, quote/production filtering, artwork proof, cancellation and no-artwork availability. No further confirmed open TS-007 finding remains. Client-only rejection of duplicate reference rows is separately fixed in FE-006; authoritative Domain acceptance/pricing is unchanged.

## Fresh verification

```powershell
$env:BREVIERP_CRM_TEST_CONNECTION='Host=127.0.0.1;Port=56403;Username=postgres;Database=postgres;Pooling=false'
dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter 'FullyQualifiedName~PrivateFileConcurrencyTests|FullyQualifiedName~PrivateFileLifecycleTests' --verbosity quiet --logger 'trx;LogFileName=cleanup-audit.trx' --results-directory E:/Development/.brevi-p4-wave2-audit-test-results
dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter 'FullyQualifiedName~CrmDependencyTests|FullyQualifiedName~CatalogApplicationDependencyTests' --verbosity quiet --logger 'trx;LogFileName=wave2-audit-architecture.trx' --results-directory E:/Development/.brevi-p4-wave2-audit-test-results
dotnet ef migrations has-pending-model-changes --no-build --project src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/Crm.Infrastructure.csproj --context CrmDbContext
```

All exit0: **9 actual PostgreSQL lifecycle/concurrency + 2 architecture cases**, zero skipped; no EF model delta. Covers quota/revision winners, finalize/cleanup overlap, concurrent cleanup, retained history state/bytes, staged replay, actual readback/tamper/artwork and unconfigured no-artwork flow. No new behavioral Red is claimed for structure-only extraction; existing scenarios prove preservation. The frontend fix separately obtained two real Red results and26 passing focused cases, giving37 passing cases across the audit's two repositories.

PostgreSQL18.0 used a new task cluster at E:/Development/.brevi-p4-wave2-audit-postgres-data, loopback56403. Fixtures create/drop unique databases and migrate twice. Production services/data were excluded. No full suite or unnecessary repeat of prior120-case evidence. No API/schema/configuration change or new own warning.

Final checks: `git diff --check` passed. SHA256 values of aggregate, private-files, custom-sewing and custom-templates contracts are unchanged from the pre-audit baseline. The temporary PostgreSQL cluster was stopped successfully and its verified task directory removed; TRX results and server log were retained. No commit or push was made during this audit.
