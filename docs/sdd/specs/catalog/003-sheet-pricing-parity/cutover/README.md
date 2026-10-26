# TS-007 — source parity and legacy price retirement

Authorized implementation: representative frozen-source diagnostics, removal of old Product-derived sale price arithmetic and Admin legacy fallback/display. No historical quote migration, production deployment, automatic input approval or price publication.

## Acceptance

- SC-R01: Product list/detail metadata reads no longer calculate old sewing/PPE sale prices or fetch references to do so. Pricing is read through getPublishedSewingProducts / getPpeProductPrices. Deprecated numeric Product sale-price fields remain zero compatibility placeholders; sewing.prices remains null. Material/supplier metadata is retained. These placeholders are never presented as valid prices by Admin.
- SC-R02: PPE without a versioned offer shows an onboarding/blocker state, never the old price. Archived percent inputs and old Sewing quantity bands are not current pricing controls. Initial supplier/base metadata may still be captured by the compatible Product write contract; only the explicit Pricing version workflow activates current prices.
- SC-R03: Read-only verification compares every price/component in six Sewing Articles spanning all five source categories and all 119 imported PPE offers. It records source/versions, expected/actual/difference, missing source/input values and strict blockers separately. Diagnostic review overrides exist only in memory. No report can publish or approve.
- SC-R04: Exact response shapes, authorization, idempotency, history and unrelated Product list/media/edit behavior remain covered by focused tests. Existing writable legacy records are retained as compatibility metadata; deleting stored data or applied migrations is not required.

## Ownership and transition

Domain owns the current Sewing/PPE engines. Remove unused Catalog.Domain.Products.Pricing arithmetic and the list-only Reference bridge after tracing all code consumers. Product list projects only metadata and media identity; it does not hydrate manufacturing details for obsolete formula inputs. Product detail maps manufacturing metadata without calculating sale prices. Keep public DTO field/type names to avoid an unrelated schema break; mark old price fields deprecated in canonical OpenAPI and document the explicit consumer transition.

Admin removes old price fallback and old-band displays. Current endpoints, generated types and permission boundaries remain. Rule/offer editor ownership stays unchanged. New Product base/supplier values are onboarding metadata, not a second current price master; percent controls are archived and hidden. Imported metadata editing limitations outside pricing remain owned by the Product/profile feature.

## Work and evidence

- TS-007A: read-only PPE diagnostic mode alongside the existing Sewing diagnostic; unit/integration checks and frozen-source report.
- TS-007B: Red tests for obsolete prices still calculated; retire arithmetic/list price-reference hydration and preserve metadata/query semantics.
- TS-007C: clean backend checkpoint/contract sync; client tests for no legacy fallback, archived controls and metadata-only manufacturing view.
- TS-007D: current maps, source difference classification and exact verification evidence. See [evidence](evidence.md).

No database migration or real-data write is planned. Diagnostic commands use the verified local target and ignored reports. The local backend checkpoint is explicitly authorized after checks; no push/frontend commit is authorized. Quotes and Storefront API adoption remain separate consumers and are not described as completed here.

[Structural audit and responsibility split](audit.md#structural-follow-up--2026-10-25) · [Structural verification](structure-verification.md).
