# Користувачі, bootstrap і registration

## Що існує зараз

У поточному API немає public self-registration endpoint-а. Session API підтримує лише login, refresh, logout і `me`.

Поточні шляхи створення користувача:

| Шлях | Поведінка |
| --- | --- |
| Startup bootstrap | `RoleSeeder` створює seed-ролі. `IdentitySeeder` створює налаштованого адміністратора лише коли його ще немає та задано `ADMIN_DEFAULT_PASSWORD`. |

Адміністратор за замовчуванням конфігурується через `AdminUserConfig`; його пароль читається з `ADMIN_DEFAULT_PASSWORD`. Ніколи не комітьте це значення в конфігурацію або документацію.

## Проєктування нової registration або user-management функціональності

Розглядайте це як функціональність Identity-модуля. Це не деталь реалізації Catalog, Reference або API-контролера.

```text
Identity.Api endpoint
  → Identity.Application command + validator
  → application service contract
  → Identity.Infrastructure implementation
  → UserManager<AppUser> / RoleManager<AppRole> / AppIdentityDbContext
```

Мінімальні рішення для специфікації функціональності:

- Хто може створювати користувача: anonymous self-registration, Admin-only, import-only або інша policy.
- Identity fields і validation: email uniqueness, normalized input, full name і password rules.
- Початкова роль і чи може її обирати викликач. Public request ніколи не може надати Admin або Manager.
- Account activation: immediate login, email confirmation, invitation або administrator approval.
- Password setup/reset і recovery design.
- Abuse controls: rate limit, audit logs і вимоги CAPTCHA/email-verification.
- Що відбувається з active sessions, коли змінюються roles, password або account state.
- API response shape: ніколи не повертайте password, refresh token, password-reset token або внутрішні Identity errors.

## Межі реалізації

- Розміщуйте HTTP contract і controller в `Identity.Api`.
- Розміщуйте command/query, FluentValidation validator, Ardalis.Result contract і application-facing interface в `Identity.Application`.
- Розміщуйте ASP.NET Core Identity calls, `AppUser` persistence, role assignment і token-provider calls в `Identity.Infrastructure`.
- Тримайте лише сталі role constants в `Identity.Domain`; не розміщуйте там HTTP DTO, `UserManager` або EF concerns.
- Реєструйте новий service через наявний Identity module registration flow; Host.Api компонує його через `AddHostServices`.

Використовуйте помилки `UserManager` для створення безпечного, узгодженого application result. Не розкривайте необроблену Identity diagnostics анонімним викликачам.
