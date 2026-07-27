# Робочий контекст для ШІ

Прочитайте цей файл перед плануванням нетривіальної backend-зміни, потім дотримуйтеся пов’язаних документів.

1. Прочитайте `docs/sdd/architecture/README.md` і `docs/sdd/standards/workflow/feature-delivery.md`.
2. Створіть або оновіть специфікацію в `docs/sdd/specs` до реалізації.
3. Зберігайте залежності всередину: `API → Application → Domain`. Infrastructure реалізує внутрішні абстракції.
4. Використовуйте Mediator для кожного сценарію, FluentValidation для input та Ardalis.Result для очікуваних outcomes.
5. Розміщуйте бізнес-інваріанти в Domain; не розміщуйте їх у контролерах, EF configurations або pipeline behaviors.
6. Вважайте публічні API-контракти стабільними. Документуйте адитивне або несумісне contract decision у специфікації.
7. Використовуйте `AsNoTracking`/projections для reads та зберігайте conventions transaction, authorization, domain events і logging.
8. Запускайте restore, build і test; повідомляйте точні failures та чи є вони передіснуючими.

Для нової функціональності спочатку прочитайте `docs/sdd/specs/_templates/feature/README.md` і скопіюйте її цільову структуру. Використовуйте `docs/sdd/specs/_templates/migration/template-migration.md` для міграції та `docs/sdd/specs/_templates/git/git-commit-batching.md` для планування комітів.

Для локального запуску, конфігурації, міграцій/seeders, Swagger-діагностики або фонових задач прочитайте `docs/sdd/operations/README.md` до початку роботи.
