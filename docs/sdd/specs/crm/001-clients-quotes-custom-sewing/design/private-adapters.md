# Private byte and document adapters — EN-003

The existing Catalog AdmTools media contract returns public URLs and lacks private scoped byte reads. Crm therefore uses a separate opt-in filesystem adapter and a configured Chromium renderer. No NuGet dependency, public ACL change or HTTP upload/issue operation is part of this readiness task.

## Persistent bytes and access

Application `Files/Access/PrivateFileService` checks `IPrivateFileAccess` before calling `IPrivateFileStore`. TS-007 must implement that grant against authoritative draft/quote ownership and register the service. API handlers must use this authorized boundary. The adapter alone is an internal byte port and cannot decide business ownership.

`FileSystemPrivateFileStore` requires an explicitly configured absolute private root and public web root. It rejects roots in public trees, broader inherited ACLs and reparse/link traversal. On Windows a newly provisioned root permits only the service identity, SYSTEM and Administrators; on Unix it requires user-only permissions. Deployment must also keep the volume outside every static-file mapping. The volume must support local filesystem atomic rename and cross-process exclusive locks; distributed/object storage needs another verified adapter.

Only canonical PDF/JPEG/PNG MIME types with matching byte signatures are accepted. The inclusive bound is 20 MiB, independent of posted length; committed files are limited to 30 per configuration under a filesystem lock. IDs are opaque GUIDs; paths and caller filenames never form public metadata. Every read verifies stored length, signature and SHA-256 against real bytes. Signature identification does not claim antivirus or complete format validation.

Writes have new IDs and commit via a pending directory rename; there is no overwrite/delete operation. TS-007 owns relational metadata, pending/link rules and immutable version references. EN-006 owns retention duration and safe cleanup decisions. Do not remove bytes referenced by historical versions.

## Typed document rendering

`IPrivateDocumentRenderer` receives only a bounded `PublicDocumentProjection` of title, paragraphs and validated inline images. `PublicDocumentHtmlWriter` escapes all text/attributes and embeds the configured TTF font and PNG/JPEG data. CSP blocks scripts, external resources and connections. There is no raw HTML or remote URL input.

`ChromiumPrivateDocumentRenderer` starts an explicitly configured executable with argument-list APIs, a fresh private profile and scratch directory, bounded timeout/output and the browser sandbox enabled. Transient source/PDF/profile are removed after each job. Hostname/background-network flags are defense in depth; production must additionally deny network egress for the renderer. The global `scriptEnabled=false` browser setting prevents printing in the tested runtime and is deliberately absent; document CSP still prohibits scripts.

Local proof uses installed Microsoft Edge 156.0.4314.50 and installed Arial, with exact hashes in dated evidence. These installations are not bundled or licensed for redistribution by this change. The existing plain ASP.NET Docker image does not contain a browser/font. Production must provision and pin a supported Chromium binary, its OS/sandbox dependencies and a redistribution-eligible Ukrainian-capable font, retaining required notices and updates. Runtime behavior follows the [official headless browser interface](https://developer.chrome.com/docs/chromium/headless).

## Explicit configuration

Register `AddCrmPrivateFileStorage(PrivateFileStorageOptions)` and `AddCrmPrivateDocumentRenderer(ChromiumDocumentOptions)` when enabling their dependent workflows. Resolve the adapters at feature startup to fail early on missing/unsafe configuration. No default volume or executable is silently selected.

| Configuration | Required value |
| --- | --- |
| Crm:PrivateFiles:RootPath | Dedicated persistent private volume, e.g. `/var/lib/brevi/crm-private-files` |
| Crm:Documents:ScratchRootPath | Separate private writable scratch, e.g. `/var/lib/brevi/crm-render-work` |
| Crm:Documents:ChromiumExecutablePath | Absolute installed/pinned browser executable |
| Crm:Documents:FontPath | Absolute installed licensed TTF font |
| PublicWebRootPath option | Actual absolute public web root supplied by Host |
| Timeout option | Bounded 1–120 seconds; default 30 seconds |

These option values contain no credentials. Readiness proof covers adapters and access-port enforcement; TS-007 file APIs and TS-010 immutable document issuance remain separate workflows. [Execution evidence](../evidence/en-003-2026-10-28.md).
