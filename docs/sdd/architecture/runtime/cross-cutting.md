# Кросфункціональний pipeline

## Mediator

Host.Api сканує Application assemblies Identity, Catalog, Reference і BuildingBlocks. Налаштований порядок pipeline:

1. request logging;
2. performance measurement;
3. FluentValidation;
4. exception handling;
5. dispatch доменних подій після успішного handler-а.

Усі точки входу сценаріїв проходять через Mediator, якщо наявний модуль не має явно задокументованого винятку.

## Валідація та помилки

Validators розміщуються поряд зі сценарієм у `Validators` та перевіряють форму input: обов’язкові значення, ranges, enum values, довжину рядка та узгоджені комбінації.

Domain-код перевіряє бізнес-інваріанти й переходи стану. Не дублюйте правило між шарами без свідомої захисної причини.

`ValidationBehavior` перетворює validation failures на `Ardalis.Result.Invalid`, коли відповідь є `Result` або `Result<T>`. Контролери зберігають наявний result-to-HTTP mapping.

## Доменні події

Сутності накопичують події через `IHasDomainEvents`. Dispatcher behavior збирає, очищує та публікує їх через Mediator після успішного handler-а. Дотримуйтеся життєвого циклу `collect → dispatch → clear`; не публікуйте події напряму з контролерів.

## Спостережуваність і безпека

Використовуйте структуровані шаблони Serilog без секретів, токенів, connection strings або чутливих payload-ів. Зберігайте correlation і cancellation. Авторизація налаштована політиками в хості; анонімний endpoint потребує явного `AllowAnonymous`, оскільки хост має fallback policy для автентифікованих.
