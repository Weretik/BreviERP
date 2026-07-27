# Identity, автентифікація та авторизація

Це архітектурна область для user identity, автентифікації, авторизації й планування access-control. Вона навмисно розміщена поза `platform`: platform описує технології, а цей розділ — як застосунок їх використовує.

```text
security/
├── authentication/  login, bearer tokens, refresh, logout і CSRF
├── authorization/   fallback protection, ролі та іменовані політики
├── users/           bootstrap users і проєктування registration
└── planning/        SDD-чекліст для функціональності з access control
```

## Читайте відповідно до задачі

- Проблема сесії, токена або refresh: [автентифікація](authentication/README.md)
- Захист endpoint-а, додавання політики чи ролі: [авторизація](authorization/README.md)
- Створення користувачів, registration або invitation: [користувачі](users/README.md)
- Планування функціональності з дозволами: [чекліст планування](planning/access-control-checklist.md)

## Ownership

```text
Identity.Api             HTTP session і майбутні identity endpoint-и
        │
Identity.Application     commands/queries, validators і service contracts
        │
Identity.Infrastructure  ASP.NET Core Identity, tokens, sessions, AppIdentityDbContext
        │
Identity.Domain          role names

Host.Api                 authentication middleware й authorization policies
```

Identity володіє акаунтами та credentials. Бізнес-модуль може захистити endpoint політикою, але не повинен створювати користувачів, перевіряти паролі чи напряму отримувати `UserManager<AppUser>`.
