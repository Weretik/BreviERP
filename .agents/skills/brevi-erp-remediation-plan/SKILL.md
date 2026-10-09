---
name: brevi-erp-remediation-plan
description: "Use when confirmed BreviERP architecture findings span multiple files, modules, features, or boundaries and need a separate remediation specification."
---

# BreviERP Remediation Plan

Перетвори systemic findings на dependency-ordered SDD; не реалізуй plan у цьому skill.

## Workflow

1. Прочитай source audit, `AGENTS.md`, affected architecture/standards і фактичний код.
2. Підтвердь root cause та affected boundaries; відокрем symptoms від systemic cause.
3. Створи remediation specification у доречному module/platform scope під `docs/sdd/specs/`, використовуючи feature structure без порожніх artifacts.
4. Зв'яжи `AF-*`, architecture requirements, `EN-*` prerequisites і remediation tasks.
5. Для task вкажи exact paths, dependencies, migration order, compatibility, API/data impact, verification і rollback.
6. Перевір readiness і ownership; запроси окреме погодження implementation.

## Guardrails

- Не створюй plan для локального one-file fix.
- Не приховуй breaking changes під refactor wording.
- Створення plan не закриває findings.
- Не змінюй runtime code у planning scope.

## Example

`Use $brevi-erp-remediation-plan to turn AF-008 and AF-011 into a platform remediation specification.`
