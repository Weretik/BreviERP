# Testing rules

## Selecting tests by behavior

Every new or changed observable behavior must link to an acceptance scenario such as `SC-001`. Choose the narrowest test level that proves the relevant risk:

- Domain rules and state transitions: Domain unit tests;
- scenario orchestration, validation, and results: Application tests;
- persistence constraints, transactions, and adapters: integration tests;
- HTTP status codes, serialization, authorization, and OpenAPI behavior: API integration or contract tests;
- layer or dependency changes: architecture tests.

Do not repeat an entire scenario at every level without a separate risk. Several focused tests can support one scenario.

## Test folders

Tests mirror the module and feature they check: see [test structure](module-rules.md#test-structure). Architecture tests check module references, Contracts-only access, folder names, and host content for every business module ([architecture tests](module-rules.md#architecture-tests)).

## Red → Green → Refactor

For new behavior:

1. **Red**: add the smallest test expressing the selected rule or scenario, run it, and confirm it fails specifically because the behavior is missing.
2. **Green**: implement the smallest complete behavior that makes the test pass without implementing unrelated scenarios.
3. **Refactor**: improve names and structure while preserving architecture boundaries, then rerun the focused tests.
4. **Regression**: run the affected test set and record the command and result.

A task is not complete until its evidence includes the test name and Red, Green, and regression results. Compilation errors, a broken test environment, or unrelated failures are not valid Red results.

## Exceptions

A test written before implementation can be omitted for documentation-only changes, generated migrations, exploratory prototypes, or infrastructure changes that cannot be meaningfully observed before a prerequisite exists. Record the reason, replacement check, and scenarios the exception enables or covers. The exception does not permit skipping final verification.

## Verification scope during implementation

| When | Minimum action |
| --- | --- |
| Local change | Run `dotnet test tests/<Project>/<Project>.csproj --filter <name>` for the affected behavior; for documentation, check links and run `git diff --check`. |
| Boundary or database change | Add the relevant `ArchitectureTests` or `IntegrationTests` project/filter; integration tests need a separate PostgreSQL instance through `BREVIERP_CATALOG_TEST_CONNECTION`. |
| CI | The [workflow](../../../.github/workflows/deploy-cloudrun.yml) builds everything and always runs the architecture tests, the host tests, the `Platform` integration tests and the tests of the selection script. A pull request runs the unit and integration tests picked by [`tools/ci/affected_tests.py`](../../../tools/ci/affected_tests.py) ([module rules](module-rules.md#test-structure)): a change in `src/Modules/<Module>/` or `tests/<Project>/<Module>/` runs `UnitTests.<Module>` and `IntegrationTests.<Module>` and those of every module whose projects reference it (read from the `.csproj` files); documentation alone runs nothing more; anything else (BuildingBlocks, hosts, jobs, build files, test support, CI files, and Catalog and Reference until stage 9 gives their tests their own folders) runs everything. Every push to `master` runs everything before deploy. Rerun the full suite locally only on explicit request. |

Check project names and available filters under `tests/` before running them.

While implementing a feature, run only automated tests for the changed feature and directly related components and contracts. Choose the narrowest available projects, filters, and commands that test the specific risk. Record commands run, results, and unverified risks.

Run a general module, application, or repository test suite, full `restore` and `build`, broad regression, coverage, or a full audit only at the user's explicit request. An unrequested full run is not a completion condition. If a required focused check is blocked or fails, name the exact command, failure point, and known impact on the result; do not hide unrelated errors.
