# Workflow реалізації feature за допомогою AI

Використовуйте цей workflow лише для прийнятої feature-специфікації. Користувач може дозволити всю feature, фазу, вибрані сценарії `SC-*` або названі задачі `TS-*`/`EN-*`.

```text
Працюй за `docs/sdd/specs/_templates/ai-feature-workflow/`.
Feature: `docs/sdd/specs/<module>/<NNN>-<feature-slug>`.
Scope: <уся feature | фаза | SC-IDs | TS/EN IDs>.
```

AI продовжує виконання всіх готових задач у дозволеному scope. Перехід між task-файлами або шарами не потребує окремої команди.

## Workflow

1. [Визначити scope і вибрати готові задачі](01-context-and-scope.md).
2. [Виконати задачі й зафіксувати результати](02-execution-and-records.md).
3. [Перевірити поведінку та передати результат](03-verification-and-handoff.md).
