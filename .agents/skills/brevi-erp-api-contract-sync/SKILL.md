---
name: brevi-erp-api-contract-sync
description: "Use when adding, changing, validating, or handing off a BreviERP public HTTP operation and its versioned OpenAPI contract."
---

# BreviERP API Contract Sync

Підтримуй feature contract, versioned OpenAPI, runtime transport, tests і frontend handoff узгодженими.

## Workflow

1. Прочитай feature `contracts/api-contract.md`, `docs/sdd/contracts/README.md`, API/security standards і affected controller/use case.
2. Узгодь route, method, stable operationId, DTO, `PagedInfo`, status/error shapes, authorization, idempotency і compatibility до transport implementation, коли практично.
3. Створи або онови `docs/sdd/contracts/<module>/<feature>.openapi.yaml`; aggregate `openapi.yaml` посилається на нього через `$ref` без duplication.
4. Додай focused Red API/contract tests, реалізуй explicit request model, Mediator call і established Ardalis.Result mapping. Для paged result не втрачай metadata.
5. Перевір runtime status, serialization, security і examples проти OpenAPI. Breaking change документує consumers, migration та rollout.
6. Онови traceability і, якщо потрібен consumer-facing flow, canonical frontend scenario під `docs/sdd/frontend-scenario/`.

## Guardrails

- Не expose entities, persistence types, secrets або internal exceptions.
- OpenAPI не скасовує host fallback authorization.
- Не змінюй frontend repository без явної авторизації.
- Не використовуй contracts із `docs/new/`.

## Example

`Use $brevi-erp-api-contract-sync to add and verify the Reference supplier-update operation.`
