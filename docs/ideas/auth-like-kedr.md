# Вход и права в Brevi по образцу Kedr

**Статус:** идея и план, не принятое решение и не реализация. Аудит кода 2026-10-28: BreviERP `8fa536b`, brevi-web `ef84d29`. Перед реализацией каждый этап нужно оформить как SDD по [правилам поставки](../sdd/standards/delivery-rules.md).

**Образец:** в Kedr такой механизм уже сделан: спецификация `docs/sdd/specs/identity/001-authentication-rollout/` в KedrStore (PR KedrStore #140 и kedr-web #15). Ниже он кратко пересказан, чтобы этот документ читался без Kedr.

**Фронтенд:** шаги для админки и витрины описаны в brevi-web, в `docs/ideas/auth-like-kedr.md`.

## 1. Коротко

В Brevi уже есть основа: ASP.NET Core Identity, роли `Admin`, `Manager`, `User`, именованные политики и правило «всё закрыто, если не открыто явно» (fallback policy). Но сейчас:

- часть админских API открыта всем, включая создание, изменение и удаление;
- в админке нет страницы входа;
- даже после входа сессия теряется при перезагрузке страницы, потому что API живёт на другом домене (`*.run.app`), чем админка (`office.brevi.com.ua`);
- после каждого деплоя все входы слетают.

Цель: сделать так же, как в Kedr. Админка и витрина входят через защищённую cookie на домене `api.brevi.com.ua`, админские API требуют роль, администратор сам заводит менеджеров на странице «Пользователи».

## 2. Что нашёл аудит

Важность: **К** критично, **В** высокая, **С** средняя.

| # | Важность | Что не так | Где в коде |
| --- | --- | --- | --- |
| 1 | К | Полностью открыты без входа (чтение и запись): товары, категории товаров (включая `GET admin`), медиатека (загрузка и удаление файлов), поставщики, детали изделий, дополнительные справочники. Любой человек из интернета может их менять. | `[AllowAnonymous]` на классе: `ProductsController`, `ProductCategoriesController`, `CatalogMediaController` (Catalog.Api); `SuppliersController`, `GarmentPartsController`, `AdditionalReferencesController` (Reference.Api) |
| 2 | В | Списки тканей, фурнитуры и операций открыты для чтения без входа (запись уже защищена). Это внутренние справочники с ценами, витрине они не нужны. | `[AllowAnonymous]` на `GET` в `FabricsController`, `GarmentAccessoriesController`, `GarmentPartOperationsController` |
| 3 | К | В админке нет страницы входа. Функция `login()` есть, но её никто не вызывает. Защищённые разделы (CRM, расчёт цен Sewing, СИЗ) без входа работать не могут. | brevi-web: `libs/admin/core/auth/src/session/auth-session.ts` |
| 4 | К | API работает на `brevierp-api-…europe-central2.run.app`, а админка на `office.brevi.com.ua`. Для браузера это разные сайты: refresh-cookie становится «сторонней» (`SameSite=None`) и часто блокируется, а CSRF-cookie, выданную `run.app`, страница админки прочитать не может. Итог: refresh почти всегда получает `400`, и после перезагрузки страницы пользователь снова не вошёл. | `SessionCookieOptions` (`SameSite.None`), `AuthSessionController.Refresh`, brevi-web `environment.prod.ts` |
| 5 | К | Ключи Data Protection нигде не сохраняются. Ими подписаны access- и refresh-токены. После деплоя или при запросе на другой экземпляр Cloud Run все токены становятся недействительными. | `IdentityDbContextExtensions` (нет `AddDataProtection().Persist…`) |
| 6 | В | Сервер доверяет заголовку `X-Forwarded-For` от кого угодно (`KnownProxies` и `KnownIPNetworks` очищены). Подставив свой IP, можно обойти ограничение 10 попыток входа в минуту. | `ForwardedHeadersRegistrationExtensions`, `RateLimitingRegistrationExtensions` |
| 7 | В | Администратор никогда не блокируется после неверных паролей (`Identity:AdminUser:LockoutEnabled = false`). Минимальная длина пароля 5 символов. Время блокировки не задано (по умолчанию Identity: 5 попыток, 5 минут). | `appsettings.json`, `IdentitySeeder`, `IdentityDbContextExtensions` |
| 8 | С | Две вкладки одновременно обновляют сессию одним refresh-токеном, сервер считает это кражей и разлогинивает пользователя на всех устройствах. Нет «окна» на повтор и защиты от гонки. | `IdentitySessionService.RefreshAsync` и обработка reuse |
| 9 | С | Выход из одной вкладки разлогинивает все устройства (отзываются все сессии и меняется security stamp). | `IdentitySessionService.LogoutAsync` |
| 10 | С | Нет способа завести менеджера, сменить роль, заблокировать или сбросить пароль. Есть только первый администратор из сидера. Если в продакшене не задан `ADMIN_DEFAULT_PASSWORD`, генерируется случайный пароль, и в логах его не будет (пишется только в Development). | `IdentitySeeder`; нет `AdminUsersController` |
| 11 | С | Опубликованные товары Sewing (`GET` в `SewingPublishedProductsController`) требуют роль `User`. Если витрина будет показывать их цены, ей нужен отдельный публичный endpoint только для чтения. Витрина уже настроена на `https://api.brevi.com.ua/storefront`, но такого адреса пока нет. | `SewingPublishedProductsController`; brevi-web `apps/storefront/src/environments/environment.production.ts` |
| 12 | С | Имена cookie скопированы из Kedr: `kedr.rt`, `kedr.csrf`. На одном домене это не мешает, но путает и в логах, и в коде фронтенда. | `SessionCookieOptions`, brevi-web `session-transport.ts` |

Что уже хорошо и остаётся: Identity и роли, fallback policy «требуется вход», политики `CanManageProducts`, `CatalogRead` и другие, `[Authorize(Roles = "Admin,Manager")]` на CRM, ограничение частоты запросов на login и refresh, проверка `lockoutOnFailure: true` при входе, хранение refresh-токенов в базе в виде хеша.

## 3. Как это устроено в Kedr (цель)

| Кто | Как входит |
| --- | --- |
| Админка (React) и витрина (Angular SSR) | Почта и пароль → сервер ставит cookie сессии `HttpOnly`, `Secure`, `SameSite=Lax` на домене API. Токен в JavaScript не попадает. Каждый изменяющий запрос (`POST`, `PUT`, `DELETE`) несёт заголовок `X-CSRF-Token`, равный CSRF-cookie. |
| Мобильное приложение | Access-токен на 15 минут и refresh-токен, который меняется при каждом обновлении. Хранятся в защищённом хранилище телефона. |

Остальные правила Kedr:

- API доступно на `api.<домен сайта>` через Cloudflare Worker, который пересылает запросы в Cloud Run и добавляет секретный заголовок. Только с этим секретом сервер верит переданному IP клиента (`TrustedProxy:SharedSecret`).
- Ключи Data Protection лежат в базе, общие для всех экземпляров. Деплой не разлогинивает.
- Любая учётная запись блокируется на 15 минут после 5 неверных паролей.
- Повтор refresh в течение 60 секунд (две вкладки) не считается кражей. Выход завершает только текущую сессию.
- Первый администратор создаётся сидером из настроек и секрета. Открытой регистрации админов нет. Администратор на странице «Пользователи» создаёт админов и менеджеров, меняет роль, блокирует, задаёт новый пароль. Себя заблокировать или понизить нельзя.
- Все админские API требуют политику (Admin или Manager). Публичный каталог, вход и refresh открыты и ограничены по частоте.
- Без входа API отвечает `401`, без нужной роли `403`. Фронтенд при `401` ведёт на страницу входа, при `403` показывает «Нет доступа» и прячет пункты меню, на которые нет прав.

**Отличие Brevi:** мобильного приложения пока нет, поэтому мобильный вход (токены в теле запроса) делать не нужно. Его можно добавить позже тем же способом, что в Kedr.

## 4. План по шагам

Порядок важен: сначала адрес API и укрепление, потом новый вход, и только когда админка умеет входить, закрывать API. Иначе админка перестанет работать.

### Шаг 0. Адрес API на домене brevi.com.ua (инфраструктура)

Без этого шага cookie-вход в браузере работать не будет (находка 4).

1. Проверить, что домен `brevi.com.ua` обслуживается в Cloudflare (DNS в Cloudflare).
2. Скопировать из KedrStore папку `deploy/cloudflare/api-proxy` (Worker) и поменять адрес на `api.brevi.com.ua`, а адрес Cloud Run на `brevierp-api`.
3. Создать один случайный секрет и положить его в двух местах: в Secret Manager для Cloud Run (`TrustedProxy__SharedSecret`) и в секрет Worker.
4. Перенести из KedrStore `TrustedProxyClientIpMiddleware` и его настройки, а `ForwardedHeaders` перестать доверять всем (находка 6).
5. Проверить `https://api.brevi.com.ua/health`. Затем переключить фронтенды на новый адрес.

Подробная инструкция с объяснением есть в KedrStore: `docs/sdd/operations/deployment/cloudflare-api-proxy.md`.

### Шаг 1. Укрепление без изменений во фронтенде

1. Сохранять ключи Data Protection в базе: `AppIdentityDbContext` реализует `IDataProtectionKeyContext`, миграция таблицы `DataProtectionKeys`, `AddDataProtection().PersistKeysToDbContext<AppIdentityDbContext>().SetApplicationName("BreviERP")` (находка 5). Миграцию выполняет job `brevierp-seed` до деплоя API; это уже так в `cloudbuild.yaml`.
2. Блокировка: 5 неверных попыток → 15 минут, для всех, включая администратора (`LockoutEnabled = true` по умолчанию, сидер выравнивает уже созданного админа) (находка 7). Пароль сотрудников не короче 8 символов.
3. Refresh: окно 60 секунд на повтор и защита от гонки (`xmin` как concurrency token в `AppRefreshSession`) (находка 8).
4. Выход завершает только текущую сессию и не меняет security stamp (находка 9).
5. Первый администратор: задать в продакшене `Identity:AdminUser:Email` и секрет `ADMIN_DEFAULT_PASSWORD` (Secret Manager для job `brevierp-seed`) (находка 10).
6. Переименовать cookie в `brevi.*` (находка 12).

### Шаг 2. Вход через cookie и пользователи

1. Схема аутентификации как в Kedr: по умолчанию политика-схема, которая при заголовке `Authorization: Bearer` использует токен (нужно для Swagger и будущего мобильного), иначе cookie Identity. Cookie `__Host-brevi.session`, `HttpOnly`, `Secure`, `SameSite=Lax`, скользящий срок 7 дней, вместо редиректа ответ `401` или `403`. Проверка security stamp раз в 5 минут.
2. `CookieCsrfMiddleware`: для запросов с cookie-сессией, кроме `GET`, `HEAD`, `OPTIONS`, требовать `X-CSRF-Token`, равный cookie `brevi.csrf` с `Domain=brevi.com.ua`.
3. Новые endpoints: `POST /api/auth/cookie/login`, `POST /api/auth/cookie/logout`; `GET /api/auth/session/me` работает для обеих схем.
4. Сотрудники: `/api/admin/users` с политикой `CanManageUsers` (роль Admin): список, создание Admin или Manager, смена роли, блокировка и разблокировка, новый пароль. Блокировка меняет security stamp и отзывает refresh-сессии. Себя заблокировать или понизить нельзя.
5. Контракты в `docs/sdd/contracts/identity/`, затем синхронизация OpenAPI во фронтенде.
6. Старые `/api/auth/session/login|refresh|logout` удалить, когда brevi-web перейдёт на cookie.

### Шаг 3. Закрыть API

Только после того, как админка умеет входить (шаг 2 во фронтенде).

1. Убрать `[AllowAnonymous]` из контроллеров находок 1 и 2. Запись: `CanManageProducts` (Manager, Admin). Чтение админских списков: `RequireManagerRole` или `CatalogRead`, как решим.
2. Для витрины сделать отдельные публичные endpoints только для чтения (например, под `/storefront/...`): опубликованные товары, категории, медиа по ссылке. Только они остаются `[AllowAnonymous]` и с ограничением частоты (находка 11).
3. Добавить `401` и `403` в контракты затронутых операций.
4. Тесты на каждый закрытый endpoint: без входа `401`, роль `User` `403`, Manager или Admin успех.

### Что не входит

Мобильный вход, вход через Google и подобное, общий вход Kedr и Brevi (SSO), двухфакторная проверка, самостоятельная регистрация, сброс пароля по почте. Для них нужны отдельные решения.

## 5. Решения, которые нужны от владельца

- **Р-1. Кто такие пользователи с ролью `User`?** Сейчас эта роль даёт чтение каталога и расчётов Sewing. Если клиентов в системе пока нет, может, роль не нужна, а витрине хватит публичных endpoints.
- **Р-2. Что видит менеджер?** В Kedr Manager может всё, кроме страницы «Пользователи». Подходит ли так же для Brevi (CRM, цены, справочники)?
- **Р-3. Домен `brevi.com.ua` в Cloudflare?** Если нет, нужно перенести DNS в Cloudflare или выбрать другой способ (Google Load Balancer стоит денег каждый месяц).

## 6. Риски

- Шаги 2 и 3 бэкенда и фронтенда нужно выкатывать вместе: после закрытия API старая админка без входа перестанет работать.
- Локальная админка (`localhost`) с продакшен-API cookie-вход не получит. Для разработки нужен локальный API.
- Cloud Run URL остаётся открытым в интернете. Защищает не Worker, а проверка прав в самих endpoints, поэтому шаг 3 обязателен.
