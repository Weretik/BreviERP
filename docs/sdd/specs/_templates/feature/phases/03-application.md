# Фаза 03: Application-сценарії

**Статус:** чернетка  
**Залежить від:** фаз 01 і 02, залежно від застосовності  
**Блокує:** публікацію API

## Результат

Кожна потрібна операція представлена сфокусованим CQRS-сценарієм із явною валідацією та `Result`-наслідками.

## Перелік потрібних операцій

Позначайте лише справді потрібні операції:

| Операція | Команда/запит | Результат | Авторизація/володіння | Потрібна? |
| --- | --- | --- | --- | --- |
| Створити | команда | ID/статус | | |
| Отримати список | запит | пагінована проєкція | | |
| Отримати за ID | запит | DTO/not found | | |
| Оновити | команда | статус/DTO | | |
| Видалити | команда | статус | | |
| Інше | | | | |

## Проєктування

- Handler для кожного сценарію:
- Власник request/response DTO: приватний DTO або `Module.Contracts`:
- Правила FluentValidation:
- Доменні/бізнес-правила, які не охоплює validator:
- Статуси Ardalis.Result:
- Межа `IUnitOfWork`/транзакції:
- Ardalis.Specification: write-специфікація, read-специфікація або пряма read-проєкція; поясніть вибір:
- Поширення cancellation:
- Вплив assembly marker:
  - Наявний модуль: підтвердьте, що наявний `ApplicationAssemblyMarker` уже сканується.
  - Новий Application-модуль: створіть `<Module>ApplicationAssemblyMarker` і додайте його до assemblies у `Host.Api` MediatorRegistrationExtensions та FluentValidationRegistrationExtensions.

## План файлів

```text
<Module>.Application/Features/<Area>/<UseCase>/
├── Command або Query
├── Handler
├── Validators/
├── DTOs/
└── Extensions/
```

## Критерії приймання

- [ ] Команди не приховані в запитах; запити не записують дані.
- [ ] Кожне вхідне правило перевірене до handler-а.
- [ ] Доменні інваріанти залишаються в Domain.
- [ ] Read-модель сфокусована на проєкції, а write-шлях використовує наявну транзакційну convention.
- [ ] Mediator може знайти handler, а FluentValidation — validator через правильний application assembly marker.

## Перевірка

- Unit-тести:
- Handler/integration-тести:
- Ризики:
