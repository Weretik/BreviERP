# Міграції та seeders

## Порядок запуску

У BreviERP міграції та початкове наповнення запускаються підтвердженим механізмом `IDatabaseMigrator`/`ISeeder`; окремим хостом для seed-операцій є `Host.Seed`.

```text
Host.Seed або погоджений startup-процес
  → зареєстровані IDatabaseMigrator / DbContext
  → зареєстровані ISeeder (один екземпляр кожного типу seeder-а)
  → завершення операції
```

Реалізовані module contexts: Catalog, Identity, Reference та Accounting. Запуск мігратора або seeder-а може змінити схему БД і вставити/оновити seed-дані. Не спрямовуйте локальний процес на БД, якщо цей ефект не є очікуваним.

## Поточні Identity seeders

- `RoleSeeder` забезпечує наявність ролей `Admin`, `Manager` і `User`.
- `IdentitySeeder` створює налаштованого адміністратора лише коли його немає та задано `ADMIN_DEFAULT_PASSWORD`.

Пароль не читається з поля `Identity:AdminUser:DefaultPassword`: seeder читає `ADMIN_DEFAULT_PASSWORD`. Відсутнє значення пропускає створення адміністратора й журналює цей стан.

## Правила змін

- Створюйте міграцію лише коли функціональність змінює persistent schema або контракт даних.
- Розміщуйте проєктування міграції та кроки rollout/verification у [шаблоні міграції](../../specs/_templates/migration/template-migration.md).
- Не редагуйте вже застосовану міграцію, щоб змінити production-історію.
- Вважайте зміни seed-даних змінами даних: документуйте ідемпотентність, поведінку з наявними даними та підхід rollback/recovery.
- Перевіряйте startup або Host.Seed логи після зміни міграції чи seeder-а.
