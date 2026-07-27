# Фаза 04: Публікація API

**Статус:** чернетка  
**Залежить від:** фази 03 та прийнятого API-контракту  
**Блокує:** ручну Swagger-перевірку та передачу frontend

## Результат

Application-сценарії опубліковані через стабільні, тонкі HTTP-endpoint-и.

## Проєктування

- Контролер і action:
- Маршрут і HTTP-метод:
- Binding запиту: route, query, body:
- Запит `ISender` і `CancellationToken`:
- `ProducesResponseType`/OpenAPI:
- Мапінг Result у HTTP:
- Політика авторизації або `AllowAnonymous`:
- Проєктування access-control (за потреби): дотримуйтесь [чекліста контролю доступу](../../../../architecture/security/planning/access-control-checklist.md); не створюйте користувачів або ролі в контролері не-Identity.
- Вимога `Idempotency-Key` для записів:
- План сумісності/адитивної або несумісної зміни:
- Вплив композиції хоста:
  - Наявний API-модуль: підтвердьте, що його API assembly marker уже зареєстровано як MVC application part.
  - Новий API-модуль: додайте project reference до Host.Api, створіть `<Module>ApiAssemblyMarker` і додайте його до `ControllersRegistrationExtensions.AddModuleControllers`.
  - Новий Application-модуль: підтвердьте, що фаза 03 додала його application marker до сканування Mediator і FluentValidation.
  - Новий Infrastructure-модуль/сервіс: зареєструйте його через наявний extension реєстрації модуля/хоста, який викликає Program через `AddHostServices`; не розміщуйте feature-реєстрацію прямо в Program, якщо Program не є встановленою точкою композиції.

## План файлів

```text
<Module>.Api/
└── Controllers/<Area>Controller.cs

Host.Api/
└── DependencyInjection/ServiceRegistration/
    ├── Web/ControllersRegistrationExtensions.cs     application part нового API-модуля
    ├── Pipeline/MediatorRegistrationExtensions.cs   marker нового Application-модуля
    ├── Pipeline/FluentValidationRegistrationExtensions.cs
    └── extension реєстрації модуля                 новий infrastructure/module service
```

## Критерії приймання

- [ ] Контролер не має бізнес-правил, EF-доступу або прямого виклику адаптера.
- [ ] Публічний маршрут і поля відповіді збігаються з прийнятим контрактом.
- [ ] Авторизація явна та використовує наявні conventions політик/ролей.
- [ ] Нова реєстрація, керування користувачами, ролями чи політиками відповідає ownership Identity і security-рішенням у прийнятій специфікації.
- [ ] OpenAPI містить endpoint і очікувані метадані відповідей.
- [ ] Host.Api компонує кожну нову API/Application/Infrastructure assembly через наявний потік реєстрації `AddHostServices`.

## Перевірка

- API integration-тести:
- Перевірка OpenAPI-контракту:
- Ризики:
