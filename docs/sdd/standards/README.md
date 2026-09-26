# Інженерні правила backend

Ця папка містить сталі правила для кожної backend-зміни. Вона не містить frontend-правил: frontend є окремим проєктом.

- [Backend rules](backend-rules.md) — шари, CQRS, DDD, Mediator і організація коду.
- [Стратегія ідентифікаторів](identifier-strategy.md) — обов'язкові правила вибору ID перед створенням або зміною сутності.
- [API rules](api-rules.md) — HTTP-контракти, валідація, результати й помилки.
- [Database rules](database-rules.md) — EF Core, читання, транзакції, міграції та дані.
- [Security and observability rules](security-observability-rules.md) — авторизація, privacy, Serilog і diagnostics.
- [Testing rules](testing-rules.md) — покриття сценаріїв, Red → Green → Refactor, вибір тестів, винятки та обов’язкові команди перевірки.
- [Delivery rules](delivery-rules.md) — behavior-first SDD flow, traceability, scope та definition of done.

Feature-специфікації посилаються на ці правила, а не дублюють їх.
