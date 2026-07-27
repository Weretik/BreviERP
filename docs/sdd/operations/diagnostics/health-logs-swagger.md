# Діагностика: health, логи та Swagger

## Health

`GET /health` мапиться `Host.Api` і явно дозволяє анонімний доступ. Використовуйте його першим, щоб встановити, що хост запущено та він досяжний.

## Swagger та OpenAPI

Swagger UI увімкнено лише коли `ASPNETCORE_ENVIRONMENT=Development`. Із HTTPS launch profile відкрийте:

```text
https://localhost:7142/swagger
```

Swagger bearer scheme очікує access token, повернений `POST /api/auth/session/login`. Вставляйте access token в авторизацію Swagger; не вставляйте й не розкривайте refresh token.

Для відтворюваного тесту функціональності заповнюйте [фазу ручного Swagger-тесту](../../specs/_templates/feature/phases/05-swagger-manual-test.md). Вона має зафіксувати маршрут, передумови, вхідні дані, очікуваний статус і випадки помилок.

## Логи та збої запуску

Serilog конфігурується з конфігурації хоста та пише до console; також підтримується Seq за наявної конфігурації. У HTTP pipeline увімкнено request logging. У разі збою запуску спочатку дослідіть console output щодо валідації конфігурації, міграції БД або seeder-ів.

Не логуйте та не поширюйте токени, паролі, повні connection strings, значення cookie чи необроблені чутливі payload-и. Для проблеми захищеного маршруту розрізняйте ці випадки до зміни коду:

| Спостереження | Перша перевірка |
| --- | --- |
| `401 Unauthorized` | Access token існує, актуальний і передано як bearer token. |
| `403 Forbidden` | Автентифікований користувач має роль/політику, потрібну endpoint-у. |
| Browser-запит заблоковано | Дозволений CORS origin, HTTPS/cookie attributes та CSRF header для refresh. |
| API падає до обслуговування запитів | Підключення до БД, міграції, seeders і обов’язкові settings. |
