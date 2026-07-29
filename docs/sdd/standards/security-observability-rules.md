# Security and observability rules

## Security та privacy

Використовуйте least privilege. Host fallback policy вимагає authentication; свідомо public endpoints позначайте `AllowAnonymous`. Зберігайте наявні role/policy conventions. Деталі моделі див. у [Identity та контролях доступу](../architecture/security/README.md).

Не логуйте tokens, passwords, connection strings, PII або sensitive payloads. Не повертайте internal exception details у production responses.

## Logs і diagnostics

Використовуйте structured Serilog messages зі стабільними, корисними properties. Зберігайте correlation, cancellation та error diagnostics. Логуйте на належному рівні без шуму.

Обробляйте transient failures явно та observably. Не вводьте implicit retries для write operations, які можуть дублювати side effects.
