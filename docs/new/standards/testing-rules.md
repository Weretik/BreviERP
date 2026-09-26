# Testing rules

## Behavior-first test selection

Every new or changed observable behavior must reference an acceptance scenario
such as `SC-001`. Select the narrowest test level that proves the risk:

- Domain rules and state transitions: Domain unit tests;
- use-case orchestration, validation, and results: Application tests;
- persistence constraints, transactions, and adapters: integration tests;
- HTTP status, serialization, authorization, and OpenAPI behavior: API
  integration or contract tests;
- layer or dependency changes: architecture tests.

Do not repeat the complete scenario at every level without a distinct risk.
One scenario may be supported by several focused tests.

## Red -> Green -> Refactor

For new behavior:

1. **Red**: add the smallest test that expresses the selected rule or scenario,
   run it, and confirm that it fails for the expected missing behavior.
2. **Green**: implement the smallest complete behavior that makes the test pass
   without implementing unrelated scenarios.
3. **Refactor**: improve names and structure while preserving architectural
   boundaries, then rerun the focused tests.
4. **Regression**: run the affected test set and record the command and result.

A task is not complete until its evidence names the test and records the Red,
Green, and regression results. Do not treat compilation errors, broken test
setup, or unrelated failures as a valid Red result.

## Exceptions

A Red-first test may be omitted for documentation-only work, generated
migrations, exploratory spikes, or infrastructure changes that cannot be
observed meaningfully before a prerequisite exists. Record the reason, the
replacement verification, and the scenarios enabled or covered. An exception
is not permission to omit final verification.

## Repository verification

Before completing a feature, run where applicable:

```powershell
dotnet restore KedrStore.sln
dotnet build KedrStore.sln --no-restore
dotnet test KedrStore.sln --no-build
```

If verification is blocked or fails, state the exact command, failure point,
and whether the failure is pre-existing or caused by the change. Do not hide
unrelated failures.
