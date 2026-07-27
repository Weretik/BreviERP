# Автентифікація

## Поточний механізм

BreviERP використовує ASP.NET Core Identity із bearer-token scheme (`IdentityConstants.BearerScheme`). Дані Identity та refresh-session records зберігаються через `AppIdentityDbContext` у PostgreSQL. `Host.Api` вмикає authentication middleware перед authorization.

```text
Client
  | POST /api/auth/session/login { email, password }
  ▼
AuthSessionController (AllowAnonymous, AuthLogin rate limit)
  ▼
SessionLoginCommand → Mediator behaviors → IdentitySessionService
  ▼
UserManager + SignInManager → AppUserClaimsPrincipalFactory
  ▼
access token у JSON response + refresh token у HttpOnly cookie
```

`AppUserClaimsPrincipalFactory` поміщає user id, user name, email, security stamp, user claims і roles до authenticated principal. Тому roles доступні authorization policies.

## Session endpoints

| Endpoint | Доступ | Input і результат |
| --- | --- | --- |
| `POST /api/auth/session/login` | `AllowAnonymous`, `AuthLogin` rate limit | Email/password. Повертає token type, access token і lifetime; записує refresh та CSRF cookies. |
| `POST /api/auth/session/refresh` | `AllowAnonymous`, `AuthRefresh` rate limit | Потребує refresh-token cookie та налаштований CSRF header, що збігається з CSRF cookie. Ротує refresh session і повертає новий access token. |
| `POST /api/auth/session/logout` | authenticated | Відкликає активні refresh sessions, оновлює security stamp, потім очищує refresh і CSRF cookies. |
| `GET /api/auth/session/me` | authenticated | Повертає current user id, email і roles. |

Refresh token ніколи не повертається в JSON response. Його cookie має `HttpOnly`; CSRF cookie читабельний браузером, щоб frontend міг надіслати його значення в налаштованому header для refresh requests.

## Правила токенів і сесій

- Access-token і refresh-token lifetimes походять з `IdentitySessionSecurityOptions` під час `AddBearerToken` registration.
- Кожен refresh token зберігається лише як SHA-256 hash у `RefreshSessions` разом із timestamps та обмеженим client context.
- Refresh sessions мають absolute й idle expiry.
- Успішний refresh ротує session: стару відкликано та пов’язано з replacement.
- Reuse відкликаного або replaced refresh token відкликає всі active refresh sessions цього користувача.
- Logout також змінює user security stamp, інвалідуючи refresh tokens, прив’язані до старого stamp.

## Поточні Identity options

- Потрібен унікальний email.
- Password length щонайменше 5 символів.
- Digit, lowercase, uppercase та non-alphanumeric characters зараз не є обов’язковими.

Це поточні runtime settings, а не рекомендація для public-registration feature. Будь-яке public registration design має явно перевірити password policy, email confirmation, abuse protection і recovery flow до реалізації.

## Використання frontend

1. Викличте `login`; зберігайте повернений access token згідно з frontend security design.
2. Надсилайте його як `Authorization: Bearer <access-token>` для protected API requests.
3. Під час refresh дозвольте браузеру надіслати cookies і додайте налаштоване CSRF-header value з CSRF cookie.
4. Під час logout викличте `logout`, поки access token ще валідний, потім відкиньте access token на клієнті.

Не розміщуйте refresh token у local storage, application logs або frontend request body.
