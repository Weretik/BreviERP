# Як доручити AI аудит backend-коду feature

Вкажіть точний каталог feature, межі дозволеної зміни та етап: перед реалізацією, перед delivery або аудит уже реалізованої feature. AI створює чи оновлює `<feature>/code-audit/audit.md` за [шаблоном](audit.template.md). Якщо потрібна тільки оцінка без правок коду, прямо вкажіть це в запиті.

## Запит для повного аудиту й виправлення в межах feature

```text
Працюй за `docs/sdd/specs/_templates/code-audit/`.
Feature: `docs/sdd/specs/<module>/<NNN>-<feature>/`.
Scope: <уся feature або точні шляхи, use cases і модулі>.
Етап: <перед реалізацією / перед delivery / аудит реалізованої feature>.

Переглянь фактичний backend-код у scope та код, від якого він залежить.
Звір відповідальності Domain, Application, Infrastructure, Api і Host з архітектурою BreviERP.
Створи або онови `<feature>/code-audit/audit.md`: точні шляхи, виявлені змішані
відповідальності, рішення та причини. Виправ лише підтверджені проблеми в scope.
Після змін запусти релевантні тести й перевірку збірки за `docs/sdd/standards/testing-rules.md`.
Запиши виконані команди, результати та невирішені ризики. Не змінюй сторонні feature.
```

## Запит для аудиту без зміни коду

```text
Працюй за `docs/sdd/specs/_templates/code-audit/`.
Feature: `docs/sdd/specs/<module>/<NNN>-<feature>/`.
Scope: <точні межі>.
Перевір фактичний backend-код і заповни `<feature>/code-audit/audit.md`.
Код не змінюй; для кожної запропонованої зміни вкажи шлях, причину й потрібну перевірку.
```

Аудит можна включити до delivery checkpoint за [AI feature workflow](../ai-feature-workflow/README.md). Вказуйте точні шляхи; формулювання «уся feature, яку ти впровадив» допустиме лише коли межі вже однозначно зафіксовані в поточній розмові та task-файлах.
