# Дані, безпека та observability

Reads використовують projections і `AsNoTracking`, де це доречно. Уникайте N+1 queries, unbounded list endpoints і непотрібної materialization. Не створюйте міграцію, якщо функціональність явно її не потребує.

Поважайте наявні transaction і concurrency boundaries. Не додавайте automatic retries до writes із побічними ефектами без задокументованої ідемпотентності.

Використовуйте least privilege. Host fallback policy вимагає authentication; позначайте свідомо public endpoints `AllowAnonymous`. Зберігайте наявні role/policy conventions. Конкретну модель authentication та authorization див. у [Identity та контролі доступу](../../architecture/security/README.md).

Використовуйте structured Serilog messages із корисними стабільними properties. Ніколи не логуйте tokens, passwords, connection strings або sensitive payloads. Зберігайте cancellation та error diagnostics.
