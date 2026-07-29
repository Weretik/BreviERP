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
6. Якщо є HTTP або integration consumer, заповніть `contracts/api-contract.md`. Після погодження створіть машиночитаний контракт для frontend у `docs/sdd/contracts/<module>/<feature>.openapi.yaml`, додайте на нього `$ref` з `docs/sdd/contracts/openapi.yaml` і лише після цього реалізовуйте API-код.
7. Пройдіть `checklist/spec-readiness.md`. Не генеруйте tasks, поки специфікація має невирішені model або contract decisions.
8. Розбийте реалізацію на конкретні task IDs у `tasks/00-readiness.md`–`tasks/05-verification.md`: кожна задача має дію, точний шлях, залежності та checkpoint фази.
9. Виконуйте фази послідовно, позначайте завершені задачі `[x]`, а результати перевірки фіксуйте у документації.
10. Перед завершенням пройдіть `checklist/delivery-readiness.md`, синхронізуйте README, requirements, design і contracts з кодом та підготуйте delivery report.

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

| Фаза | Результат |
| --- | --- |
| [00 — Уточнення](tasks/00-readiness.md) | погоджені scope і contract |
| [01 — Domain](tasks/01-domain.md) | модель та unit-тести |
| [02 — Infrastructure](tasks/02-infrastructure.md) | persistence і міграція |
| [03 — Application](tasks/03-application.md) | CQRS use-cases |
| [04 — API](tasks/04-api.md) | endpoints і API tests |
| [05 — Verification](tasks/05-verification.md) | докази delivery |
