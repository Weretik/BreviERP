# TS-008 — imported pricing readiness and controlled activation

Status: specified before implementation, 2026-10-26. TS-008A/B implemented and verified locally; TS-008C source review and DR-C01 are delivered with explicit 724/931 exclusions; TS-008D confirmation/activation is complete locally; see [execution evidence](execution-evidence.md). See [execution evidence](evidence.md) and [readiness result](readiness-result.md). TS-007 source parity/current-consumer transition is the prerequisite. This extension uses the existing Catalog/Reference pricing owners; it adds no business module.

## Goal and scope

Produce a complete, repeatable local readiness inventory of every supported row in the immutable imported workbook, not just the six representative Sewing Articles. Separate strict current calculation readiness, source issues, input approval and existing publication. Then resolve data gaps and activate prices through existing manager workflows with version/concurrency evidence.

The initial implementation slice was TS-008A/B: a read-only report job and local report execution. The user subsequently authorized TS-008C source review and accepted DR-C01. TS-008D describes subsequent confirmation/activation, requiring revalidated reviewed data and authenticated manager execution. Neither arithmetic parity nor this implementation request fabricates those business decisions. Production release, live Google/XML refresh, quote migration, Storefront API adoption and blanket approval are excluded.

- [Rules and acceptance](requirements.md)
- [Design and responsibility map](design.md)
- [Tasks and activation checklist](tasks.md)
- [Traceability and evidence](evidence.md)

Follow [delivery](../../../../standards/delivery-rules.md), [testing](../../../../standards/testing-rules.md) and [data operations](../../../../operations/data/migrations-and-seeding.md).

[TS-008C source review design and acceptance](review.md) · [Review execution](review-evidence.md).

[Accepted review decisions and TS-008D handoff](review-decisions.md).

[TS-008D authorized execution](execution.md) · [Standing 724/931 decisions](data-decisions.md).
