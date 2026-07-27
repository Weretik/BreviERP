# Експлуатація

Цей розділ пояснює, як налаштовано, запускається та діагностується backend, що працює. Він описує експлуатаційні факти, а не проєктування функціональності; перевірки, специфічні для функціональності, залишаються у її специфікації.

```text
operations/
├── local-development/  запуск API та локальні URL
├── configuration/      джерела конфігурації й робота з секретами
├── data/               автоматичні міграції та seeders
├── diagnostics/        health, Swagger і логи
└── jobs/               підтверджені фонові задачі та їхні runbook-и
```

## Читайте відповідно до задачі

- Запустити backend локально: [локальна розробка](local-development/run-api.md)
- Налаштувати машину або виправити відсутні settings: [конфігурація та секрети](configuration/configuration-and-secrets.md)
- Зрозуміти, що startup змінює в базі: [міграції та seeders](data/migrations-and-seeding.md)
- Перевірити запущений API або відтворити endpoint: [діагностика](diagnostics/health-logs-swagger.md)
- Дослідити підтверджену фонову задачу: [задачі та runbook-и](jobs/README.md)

Не розміщуйте в цій документації пароль, токен, повний connection string або production endpoint.
