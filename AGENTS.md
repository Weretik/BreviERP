# BreviERP: інструкції для AI

Ці інструкції застосовуються до всього репозиторію.

## Навігація репозиторієм

1. Перед зміною меж модулів або шарів прочитайте `docs/sdd/architecture/README.md`.
2. Прочитайте релевантні файли з `docs/sdd/standards/README.md`. Перед створенням або зміною ідентифікатора сутності обов'язково прочитайте `docs/sdd/standards/identifier-strategy.md`.
3. Для нової feature або міграції до реалізації створіть чи оновіть специфікацію в `docs/sdd/specs/`.
4. Для публічної HTTP-поведінки дотримуйтеся `docs/sdd/contracts/README.md` і синхронізуйте feature contract, versioned OpenAPI, runtime-поведінку та тести.
5. Для запуску, конфігурації, міграцій, діагностики або seed/jobs прочитайте `docs/sdd/operations/README.md`.
6. Сталі бізнес-терміни беріть із `docs/product/glossary.md`.

`docs/sdd/` є канонічною backend-документацією. `docs/new/` містить неадаптовані матеріали іншого проєкту й не є джерелом правил або шаблонів BreviERP; не змінюйте та не використовуйте його без окремого запиту користувача.

## Архітектура та delivery

- Зберігайте межі модульного моноліту: бізнес-модулі не залежать від внутрішньої реалізації інших модулів, а залежності спрямовані до Domain.
- Не розміщуйте бізнес-правила в controllers, EF mappings, hosts або `BuildingBlocks`.
- Реалізуйте погоджену поведінку через dependency-ready задачі `TS-*` і `EN-*`. Виконуйте Red → Green → Refactor → Regression і записуйте свіже evidence.
- Зміни схеми та seed-даних проходять через погоджений механізм `IDatabaseMigrator`/`ISeeder` і окремий `Host.Seed`; не запускайте state-changing операції проти непогодженої БД.
- Оновлюйте документацію та контракти в тому самому change set, що й описану ними поведінку.
- Зберігайте сторонні зміни користувача в dirty worktree.

## Межі виконання для AI

Коли користувач дозволив реалізацію feature, фази, сценарію або впорядкованого набору задач, виконуйте всі готові задачі в погоджених межах без окремої команди для кожного task-файла. Зупиняйтеся на межі дозволеного scope, невирішеному product-рішенні, задокументованому blocker-і або перед дією, що потребує нових повноважень.

## Git

- Не створюйте commits, не push-те, не створюйте pull request і не змінюйте remote state без явного запиту користувача.
- Якщо commits дозволено, дотримуйтеся `docs/sdd/specs/_templates/git/git-commit-batching.md`: робіть невеликі reviewable commits за наміром, тримайте нерозривні model/configuration/migration зміни разом і не включайте сторонні зміни worktree.

## Repo-local skills

Використовуйте відповідний skill із `.agents/skills/`, коли спрацьовує його trigger:

- `brevi-erp-feature-spec` — створення або оновлення feature specification до коду.
- `brevi-erp-feature-delivery` — реалізація погодженої специфікації.
- `brevi-erp-feature-orchestrator` — координація явно запитаної multi-agent реалізації.
- `brevi-erp-change-verification` — вибір і запуск risk-appropriate перевірок.
- `brevi-erp-code-audit` — аудит архітектури та implementation evidence.
- `brevi-erp-remediation-plan` — планування системного виправлення архітектури.
- `brevi-erp-api-contract-sync` — синхронізація HTTP-поведінки та OpenAPI.
- `brevi-erp-database-change` — проєктування й перевірка EF Core migration або seed change.
- `brevi-erp-pr-handoff` — підготовка review-ready handoff.
- `brevi-erp-docs-sync` — пошук і виправлення підтвердженого documentation drift.

## Правила документації

- `docs/sdd/architecture/` і `docs/sdd/standards/` містять сталі правила; feature-specific рішення належать у `docs/sdd/specs/<module>/<NNN>-<feature>/`.
- Машиночитані публічні API-контракти зберігайте в `docs/sdd/contracts/`.
- Один документ відповідає за одну тему. Не створюйте порожні, дублюючі або монолітні файли.
- Перевіряйте відносні Markdown-посилання після переміщення чи перейменування файлів.
