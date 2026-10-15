# Користувачі, bootstrap і registration

## Що існує зараз

У поточному API немає публічної кінцевої точки самостійної реєстрації. API сеансів підтримує лише вхід, оновлення, вихід і `me`.

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

- Хто може створювати користувача: анонімна самостійна реєстрація, лише Admin, лише імпорт або інша політика.
- Поля Identity та валідація: унікальність email, нормалізоване введення, повне ім’я та правила паролів.
- Початкова роль і чи може її обирати викликач. Public request ніколи не може надати Admin або Manager.
- Активація облікового запису: негайний вхід, підтвердження email, запрошення або схвалення адміністратором.
- Налаштування й скидання пароля та сценарій відновлення.
- Захист від зловживань: обмеження частоти, журнали аудиту та вимоги CAPTCHA/підтвердження email.
- Що відбувається з активними сеансами, коли змінюються ролі, пароль або стан облікового запису.
- Форма відповіді API: ніколи не повертайте пароль, токен оновлення, токен скидання пароля або внутрішні помилки Identity.

## Межі реалізації

- Розміщуйте HTTP contract і controller в `Identity.Api`.
- Розміщуйте command/query, FluentValidation validator, Ardalis.Result contract і application-facing interface в `Identity.Application`.
- Розміщуйте виклики ASP.NET Core Identity, збереження `AppUser`, призначення ролей і виклики провайдера токенів в `Identity.Infrastructure`.
- Тримайте лише сталі константи ролей в `Identity.Domain`; не розміщуйте там HTTP DTO, `UserManager` або залежностей від EF.
- Реєструйте новий сервіс через наявний порядок реєстрації модуля Identity; Host.Api компонує його через `AddHostServices`.

Використовуйте помилки `UserManager` для створення безпечного, узгодженого результату Application. Не розкривайте необроблену діагностику Identity анонімним викликачам.
