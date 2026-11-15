# ADR-0008: Business rules return a Result with a typed error; exceptions only for the unexpected

**Status:** accepted · **Date:** 2026-11-15 (recorded from the current code; rule codified on 2026-11-14) · **Owner:** Виталий (project owner)

## Context

Most failures in Brevi are normal business answers: a fabric is used by a model, a КП is already sent, an accounting period is closed. Staff read the messages in Ukrainian. When such rules threw exceptions, status codes were chosen in many places and some answers showed exception text.

## Options

- Exceptions for business rules plus a mapping middleware: rejected; hidden control flow, uncaught cases become 500.
- Each endpoint chooses its own status code: rejected; the same error got different codes.
- `Result` with a typed error and one mapper: chosen.

## Decision

- Domain and Application return `Ardalis.Result` with a typed error from `<Aggregate>Errors` (stable code plus Ukrainian message). They never reference HTTP.
- Every input has a FluentValidation validator beside its use case, with Ukrainian messages; the validation behavior stops invalid input before the handler.
- One mapper in `BuildingBlocks.Api/Errors/ResultToHttpResult.cs` turns results into responses: validation 400, not found 404, conflict 409, forbidden 403, else 500, body `ProblemDetails`.
- Exceptions are only for programmer errors and the unexpected. One `GlobalExceptionHandler` logs them and answers without exception text.

## Consequences

- A handler shows every failure it can return; the same error has the same code everywhere.
- Use cases without validators are listed as known deviations in the [module rules](../../standards/module-rules.md).

## Sources

`src/BuildingBlocks/BuildingBlocks.Api/Errors/`, [module rules: errors and validation](../../standards/module-rules.md#validation).
