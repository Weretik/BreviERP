# Сеанс користувача

**Статична звірка з кодом:** 2026-10-16. **Модуль:** identity.

Login, refresh, logout і me.

## Будова й зв'язки

[AuthSessionController](../../../../src/Modules/Identity/Identity.Api/Identity.Api/Controllers/AuthSessionController.cs) викликає [login handler](../../../../src/Modules/Identity/Identity.Application/Identity.Application/Features/Auth/SessionLogin/SessionLoginCommandHandler.cs) та сусідні handlers. [IdentitySessionService](../../../../src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure/Services/IdentitySessionService.cs) і [Identity DbContext](../../../../src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure/DataBase/AppIdentityDbContext.cs) відповідають за стан сеансу та користувача; правила cookie й CSRF дивись у [потоці](../../architecture/security/authentication/session-flow.md).

## Структура в коді

Наведено перевірені точки входу та ключові складові фічі; дерево не перелічує всі допоміжні файли.

```text
src/
└── Modules/Identity/
    ├── Identity.Api/Identity.Api/Controllers/AuthSessionController.cs  # HTTP-вхід
    ├── Identity.Application/Identity.Application/Features/Auth/
    │   ├── SessionLogin/SessionLoginCommandHandler.cs  # обробник сценарію
    │   ├── SessionLogout/SessionLogoutCommandHandler.cs  # обробник сценарію
    │   ├── SessionMe/SessionMeQueryHandler.cs  # обробник сценарію
    │   └── SessionRefresh/SessionRefreshCommandHandler.cs  # обробник сценарію
    └── Identity.Infrastructure/Identity.Infrastructure/
        ├── DataBase/AppIdentityDbContext.cs  # контекст даних
        └── Services/IdentitySessionService.cs  # інфраструктурна реалізація
```

## Схема будови

```mermaid
flowchart LR
    A["AuthSessionController"] --> B["SessionLoginCommandHandler та інші handlers"]
    B --> C["IdentitySessionService"]
    C --> D["AppIdentityDbContext"]
    A --> E["Cookie та CSRF"]
```

## Пов'язані джерела

[OpenAPI](../../contracts/identity/session.openapi.yaml).
