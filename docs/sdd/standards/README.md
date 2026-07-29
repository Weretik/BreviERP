# Інженерні правила backend

Ця папка містить сталі правила для кожної backend-зміни. Вона не містить frontend-правил: frontend є окремим проєктом.

- [Backend rules](backend-rules.md) — шари, CQRS, DDD, Mediator і організація коду.
- [API rules](api-rules.md) — HTTP-контракти, валідація, результати й помилки.
- [Database rules](database-rules.md) — EF Core, читання, транзакції, міграції та дані.
- [Security and observability rules](security-observability-rules.md) — авторизація, privacy, Serilog і diagnostics.
- [Testing rules](testing-rules.md) — вибір тестів і обов’язкові команди перевірки.
- [Delivery rules](delivery-rules.md) — SDD flow, scope та definition of done.

Feature-специфікації посилаються на ці правила, а не дублюють їх.
