# <NNN> — <назва feature>

**Модуль:** <module>
**Тип:** <feature | міграція>
**Статус:** чернетка
**Власник:** <команда>
**Створено:** РРРР-ММ-ДД

<Один абзац: цінність і межі feature.>

## Порядок написання специфікації

1. Створіть папку `docs/sdd/specs/<module>/<NNN>-<feature-slug>/` і заповніть цей `README.md`: назву, модуль, власника, мету та межі.
2. Заповніть `requirements/overview.md`, а потім створіть окремий `requirements/<topic>.md` для кожної незалежної предметної області.
3. Позначте невизначеність як `[NEEDS CLARIFICATION: <питання>]`; не припускайте бізнес-правила, API або модель даних.
4. Закрийте кожен `[NEEDS CLARIFICATION]` або явно винесіть його за межі feature.
5. Заповніть `design/domain.md`, `design/infrastructure.md` і `data-model.md`. Вимоги відповідають на «що», design — на «як».
6. Якщо є HTTP або integration consumer, до початку коду погодьте перелік операцій і заповніть `contracts/api-contract.md` настільки, щоб реалізувати endpoints. У code-first workflow після реалізації API створіть машиночитаний контракт для frontend у `docs/sdd/contracts/<module>/<feature>.openapi.yaml`, додайте на нього `$ref` з `docs/sdd/contracts/openapi.yaml` і звірте його з кодом. Не передавайте API consumer до завершення OpenAPI-фази.
7. Пройдіть `checklist/spec-readiness.md`. Не генеруйте tasks, поки специфікація має невирішені model або contract decisions.
8. Розбийте реалізацію на конкретні task IDs: кожна задача має дію, точний шлях, залежності та checkpoint фази. Головні фази `00–05` лише створюють потрібні підфази. Копіюйте потрібні шаблони в папку відповідного шару: `tasks/readiness/00.N-<назва>.md`, `tasks/domain/01.N-<назва>.md`, `tasks/infrastructure/02.N-<назва>.md`, `tasks/application/03.N-<назва>.md`, `tasks/api/04.N-<назва>.md`, `tasks/verification/05.N-<назва>.md`. Не створюйте файли для виключених slices/use cases; для HTTP/integration feature API-підфази виконуються в порядку controllers → HTTP behavior → OpenAPI → API tests.
9. Виконуйте фази послідовно, позначайте завершені задачі `[x]`, а результати перевірки фіксуйте у документації.
10. Перед завершенням пройдіть `checklist/delivery-readiness.md`, синхронізуйте README, requirements, design і contracts з кодом та підготуйте delivery report.

## Правило CLI-first

Для операцій, які може виконати наявна CLI-команда, generator або script проєкту, використовуйте його замість ручного створення артефактів: migrations, SQL scripts, OpenAPI validation/generation, restore, build, tests і format/lint. Спочатку перевіряйте наявні команди в репозиторії та документації; не вигадуйте нову автоматизацію, якщо у проєкті вже є прийнятий механізм. Ручні зміни допустимі лише для логіки, контрактів або винятків, які generator не може коректно виразити, із явною причиною в задачі.

## Правило дрібних підфаз

Усі головні фази `00 — Readiness`, `01 — Domain`, `02 — Infrastructure`, `03 — Application`, `04 — API` і `05 — Verification` є лише orchestration-фазами: вони визначають потрібні підфази та порядок виконання. Не додавайте до головного файлу фази велику реалізаційну задачу чи всі деталі одним списком.

Кожна конкретна відповідальність має бути окремим файлом підфази:

- `tasks/readiness/00.1-<назва>.md`, `00.2-<назва>.md` і далі — scope, рішення та tooling;
- `tasks/domain/01.1-<назва>.md`, `01.2-<назва>.md` і далі — aggregate, value object, events або tests;
- `tasks/infrastructure/02.1-<назва>.md`, `02.2-<назва>.md` і далі — persistence, migration, read model, integration або tests;
- `tasks/application/03.1-<назва>.md`, `03.2-<назва>.md` і далі — contracts або один завершений Application use case;
- `tasks/api/04.1-<назва>.md`, `04.2-<назва>.md` і далі — controllers, HTTP behavior, OpenAPI або API tests;
- `tasks/verification/05.1-<назва>.md`, `05.2-<назва>.md` і далі — build/tests або delivery documentation.

Один файл підфази — одна зрозуміла відповідальність. Не об'єднуйте декілька незалежних use cases, усі controllers, всю OpenAPI-документацію або всі перевірки в одну велику задачу, якщо їх можна виконати й перевірити окремо.

## Вимоги

- [Огляд і межі](requirements/overview.md)
- [<предметна область>](requirements/topic.md)

## Технічний дизайн

- [Domain](design/domain.md)
- [Infrastructure](design/infrastructure.md)
- [Модель даних](data-model.md)

## Delivery

- [API/integration contract](contracts/api-contract.md)
- [Реєстр машиночитаних API-контрактів](../../../contracts/README.md)
- [Spec readiness](checklist/spec-readiness.md)
- [Delivery readiness](checklist/delivery-readiness.md)

## Задачі реалізації для AI

AI виконує лише незавершені задачі поточної фази, позначає виконані `[x]` і переходить далі лише після checkpoint. `[P]` означає безпечне паралельне виконання після залежностей.

Повна навігація файлами фаз: [tasks/README.md](tasks/README.md).

### Обов'язковий порядок фаз для HTTP/API feature

Після Domain, Infrastructure та всіх погоджених Application use cases AI **обов'язково** створює в `tasks/` окремі пронумеровані фази з шаблонів `tasks/api/` у такому порядку:

1. `NN-api-controllers.md` — перевірити `Program.cs`/composition root, підключити controllers лише за потреби, створити `<Feature>Controller` і endpoint-метод для кожного погодженого use case.
2. `NN-api-http-behavior.md` — створити API contracts і mappers лише за потреби, налаштувати authentication/authorization та використати наявний `Ardalis.Result` → `ToActionResult` mapping.
3. `NN-api-contract-documentation.md` — задокументувати фактичні endpoints в `api-contract.md` і versioned OpenAPI YAML.
4. `NN-api-tests.md` — додати HTTP/integration-тести, включно з authorization.
5. `NN-verification.md` — виконати build, tests, delivery checklist і delivery report.

Не пропускати ці API-фази, якщо feature має HTTP endpoint або integration API. Не створювати їх лише для feature без HTTP/integration surface.

| Фаза | Результат |
| --- | --- |
| [00 — Уточнення](tasks/00-readiness.md) | погоджені scope, CQRS use cases і contract outline |
| [01 — Планування Domain](tasks/01-domain.md) | потрібні й виключені Domain slices |
| [02 — Планування Infrastructure](tasks/02-infrastructure.md) | потрібні й виключені Infrastructure slices |
| [01.NN — Domain: aggregate](tasks/domain/01.NN-aggregate.template.md) | aggregate root, інваріанти й unit-тести |
| [01.NN — Domain: value object](tasks/domain/01.NN-value-object.template.md) | immutable value object і unit-тести |
| [01.NN — Domain: events](tasks/domain/01.NN-domain-events.template.md) | події лише для side effects між aggregates |
| [01.NN — Domain: tests](tasks/domain/01.NN-domain-tests.template.md) | повне покриття змінених Domain rules |
| [02.NN — Infrastructure: persistence](tasks/infrastructure/02.NN-persistence-mapping.template.md) | EF mapping, constraints, indexes і tests |
| [02.NN — Infrastructure: migration](tasks/infrastructure/02.NN-migration.template.md) | перевірена migration та rollout/rollback |
| [02.NN — Infrastructure: read model](tasks/infrastructure/02.NN-read-model.template.md) | query service/read repository і tests |
| [02.NN — Infrastructure: integration/outbox](tasks/infrastructure/02.NN-integration-outbox.template.md) | зовнішня інтеграція, delivery і retry |
| [02.NN — Infrastructure: tests](tasks/infrastructure/02.NN-infrastructure-tests.template.md) | integration-тести критичної поведінки |
| [03 — Application](tasks/03-application.md) | план підфаз 03.1, 03.2… |
| [03.NN — Application subphase](tasks/application/) | contracts або один завершений use case з validation/results/tests |
| [04 — API](tasks/04-api.md) | план підфаз 04.1, 04.2… |
| [04.NN — API subphase](tasks/api/) | controllers, HTTP behavior, OpenAPI або API tests |
| [05 — Verification](tasks/05-verification.md) | build, tests, delivery checklist і report |
