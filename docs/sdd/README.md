# BreviERP backend SDD documentation map

This section contains facts and rules specific to BreviERP. For a new task, choose the relevant section first, then check current code, configuration, and contracts. Specification status is not implementation status.

| Question | Source |
| --- | --- |
| Which modules, hosts, layer boundaries, and execution flows exist? | [Architecture](architecture/README.md) |
| Which technologies and direct NuGet dependencies are installed? | [Technology stack](architecture/platform/technology-stack.md) and [package catalog](architecture/platform/nuget-packages.md) |
| Which stable engineering rules apply? | [Standards](standards/README.md) |
| How do I start, configure, verify, and operate the system? | [Operations](operations/README.md) |
| Which public HTTP contract is implemented, and what should be handed to frontend? | [API contracts](contracts/README.md) |
| Which requirements, decisions, tasks, and evidence belong to a specific feature? | [Specifications](specs/README.md) |
| Which features are implemented, and where are their code and data? | [Feature map](features/README.md) |
| Which backend scenario is handed to frontend? | [Frontend scenarios](frontend-scenario/README.md) |

Record a decision that changes multiple modules or a long-term rule in the [architecture decision log](architecture/decisions/README.md). Local decisions for a particular feature stay in its specification. Do not duplicate general templates in this repository; documents should link only to their own files and skills available by name.
