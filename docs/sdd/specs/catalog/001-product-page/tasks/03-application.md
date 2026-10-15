# Фаза 03 — Application

> Фаза лише впорядковує підфази. Кожен Product use case має окремий файл `03.N`.

- [x] O03-01 Завершити контракти й абстракції Reference/Media: [03.1-contracts-and-reference.md](application/03.1-contracts-and-reference.md).
- [x] O03-02 Зафіксувати реалізовані обробники створення/оновлення: [03.2-create-and-update.md](application/03.2-create-and-update.md).
- [x] O03-03 Завершити production-ready read use cases: [03.3-admin-read-model.md](application/03.3-admin-read-model.md).
- [x] O03-04 Зафіксувати реалізований delete use case: [03.4-delete-product.md](application/03.4-delete-product.md).
- [x] O03-05 Завершити тести валідації, обробників і проєкцій: [03.5-application-tests.md](application/03.5-application-tests.md).
- [x] O03-06 Локалізувати та повністю покрити validation Product use cases: [03.6-validation-localization.md](application/03.6-validation-localization.md).
- [x] O03-07 Закрити production-hardening risks Application: [03.7-production-hardening.md](application/03.7-production-hardening.md).
- [x] O03-08 Збагатити Product list/detail read models для погодженого admin frontend contract: [03.8-catalog-read-model-enrichment.md](application/03.8-catalog-read-model-enrichment.md).
- [x] O03-09 Оптимізувати admin Product list query після EF collection warning: [03.9-admin-list-query-performance.md](application/03.9-admin-list-query-performance.md).
- [x] O03-10 Замінити повний Reference snapshot вузьким pricing reader для admin list: [03.10-admin-list-pricing-reference-reader.md](application/03.10-admin-list-pricing-reference-reader.md).

## Контрольна точка

Кожен сценарій використання Product має окремий обробник і `Ardalis.Result`. Список виконує фільтрацію/сортування/пагінацію на боці БД; свідчення тестів валідації, обробників і проєкцій зафіксовані у `03.5`, локалізована модель валідації — у `03.6`, посилення для промислового середовища щодо валідації категорій, ID фото, конкурентних конфліктів, проєкції деталей і перевірок архітектури — у `03.7`, а URL Media й ціни списку/деталей — у завершеній `03.8`.

## Наступна фаза

[04 — API](04-api.md)
