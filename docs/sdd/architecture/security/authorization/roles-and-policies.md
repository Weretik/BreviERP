# Авторизація

## Базове правило

`Host.Api` налаштовує fallback authorization policy, що вимагає автентифікованого користувача. Отже endpoint захищений, якщо він свідомо не оголошує `[AllowAnonymous]`. Анонімний доступ має бути явним в API-контракті та перевірятися у межах функціональності.

Автентифікація відповідає на питання **хто викликає**. Авторизація відповідає, **чи може цей викликач виконати операцію**.

## Ролі

Константи ролей розміщені в `Identity.Domain/Constants/AppRoles.cs`.

| Роль | Поточне значення | Створюється seeder-ом |
| --- | --- | --- |
| `Admin` | Адміністрування системи | так |
| `Manager` | Доступ контент-менеджера | так |
| `User` | Звичайний автентифікований користувач | так |

## Політики

Назви політик розміщено в `Identity.Application/Security/Policies/PolicyNames.cs`; їхні role requirements компонуються у `Host.Api/AuthorizationPoliciesRegistrationExtensions`.

| Політика | Дозволені ролі |
| --- | --- |
| `RequireAdminRole` / `CanManageUsers` | Admin |
| `RequireManagerRole` / `CanManageProducts` / `CanManageOrders` | Manager, Admin |
| `CatalogRead` / `OrderCreate` | User, Manager, Admin |

## Захист endpoint-а

Віддавайте перевагу наявній іменованій політиці, що виражає операцію. Контролер лише оголошує доступ; команда/запит усе одно проходить через `ISender`, а бізнес-інваріанти залишаються у Domain/Application. Не використовуйте role check у контролері як заміну політиці.

Для public endpoint використовуйте `[AllowAnonymous]` і зафіксуйте причину в контракті функціональності. Це перевизначає fallback policy.

## Додавання політики або ролі

1. Перевірте, чи наявна політика вже охоплює бізнес-операцію.
2. Якщо ні, визначте назву політики в Identity Application та role requirement у Host.Api.
3. Якщо потрібна нова роль, додайте константу в Identity Domain і зробіть startup seeding свідомим; визначте, які наявні й майбутні користувачі її отримають.
4. Захистіть endpoint новою політикою, оновіть API-контракт і вручну перевірте forbidden та permitted requests.

Додавання ролі — це зміна авторизації, а не просто зміна рядка: вона впливає на bootstrap, призначення користувачів, документацію та rollout.
