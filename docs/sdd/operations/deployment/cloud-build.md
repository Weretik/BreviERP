# Контейнеризація та конфігурація Cloud Build

**Статус перевірки:** 2026-10-15, статичний огляд файлів репозиторію. Цей документ описує налаштований конвеєр; фактичний стан ресурсів Google Cloud і останнє успішне розгортання не перевірялися.

## Джерела

- [`cloudbuild.yaml`](../../../../cloudbuild.yaml) визначає порядок збирання, публікації образів, міграцій і розгортання.
- [Dockerfile Host.Api](../../../../src/Bootstrapper/Host.Api/Dockerfile) та [Dockerfile Host.Seed](../../../../src/Bootstrapper/Host.Seed/Dockerfile) визначають образи на базі .NET 10 SDK/runtime.
- [`Host.Seed/Program.cs`](../../../../src/Bootstrapper/Host.Seed/Program.cs) викликає `MigrateAppAsync()` і `SeedAppAsync()`; [`Host.Api/Program.cs`](../../../../src/Bootstrapper/Host.Api/Program.cs) запускає HTTP-хост.

## Налаштована послідовність

1. Docker збирає образи `host-api` та `host-seed` із відповідних Dockerfile і публікує їх до Artifact Registry з тегом `$COMMIT_SHA`.
2. Cloud Build створює або оновлює Cloud Run Job `Host.Seed`: одне завдання, паралельність 1, максимум один повтор.
3. Cloud Build виконує Job і чекає завершення. Лише після цього запускає крок розгортання `Host.Api` у Cloud Run.
4. Крок розгортання API додає два посилання на Secret Manager для `AdmToolsStorage__Login` і `AdmToolsStorage__Password`; інші потрібні змінні й секрети слід перевіряти в конфігурації середовища. Сам файл не доводить, що вони вже налаштовані.

Стандартні значення параметрів підстановки у `cloudbuild.yaml`: регіон `europe-central2`, репозиторій Artifact Registry `api`, сервіс Cloud Run `brevierp-api`, Job `brevierp-seed`. Це значення **конфігурації**, а не перевірка фактичного хмарного середовища. Для іншого середовища перевірте параметри підстановки й права доступу до Artifact Registry, Cloud Run, Secret Manager та PostgreSQL.

## Межі й перевірка

- Dockerfile API виставляє порт `8080`; локальні профілі `https` і `http` описані окремо в [інструкції запуску](../local-development/run-api.md).
- Запуск `Host.Seed` може змінити схему й дані БД. Перед повторним запуском перевірте цільову БД, резервне копіювання, результати попереднього Job та [правила міграцій](../data/migrations-and-seeding.md).
- У конфігурації Cloud Build немає кроку запуску тестів. Потрібні команди перевірки визначають [правила тестування](../../standards/testing-rules.md); їх виконання не слід припускати за самим фактом запуску конвеєра.
- Після розгортання перевірте статус Cloud Build, Cloud Run Job і сервісу, журнали обох хостів, `GET /health` та контрактні сценарії відповідної функціональності. Без результатів цих перевірок не заявляйте про успішний запуск.
