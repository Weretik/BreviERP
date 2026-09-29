# Аудит відповідальностей backend-коду feature

Це додатковий модуль SDD для перевірки структури наявного й зміненого backend-коду в межах однієї feature. Він застосовується разом із [шаблоном feature](../feature/README.md) і [AI workflow](../ai-feature-workflow/README.md), але не входить до каталогу `feature/` за замовчуванням.

BreviERP — модульний ASP.NET Core backend. Аудит звіряє фактичний код із [межами шарів](../../../architecture/overview/layers.md), [структурою проєкту](../../../architecture/overview/project-structure.md) і [backend-правилами](../../../standards/backend-rules.md): `Domain`, `Application`, `Infrastructure`, `Api`, `Host.Api`/`Host.Seed` та `BuildingBlocks`. Перевіряйте лише шари й модулі, яких торкається feature.

Скопіюйте [audit.template.md](audit.template.md) до `<feature>/code-audit/audit.md`. Перед реалізацією заповніть частину про наявний код і плановані межі; перед delivery checkpoint перевірте весь код feature у scope, внесіть фактичні рішення та докази перевірки. Для вже створеної feature додайте лише `code-audit/`, не копіюючи повторно основний шаблон.

Аудит має відповісти для кожної змішаної або спірної відповідальності: залишити код цілісним, розділити в межах наявного use case чи перенести до власного шару/модуля. Запишіть конкретні шляхи, причину рішення та його вплив на публічні контракти й залежності. Кількість рядків сама по собі не вимагає поділу. Не виконуйте сторонній рефакторинг поза погодженим scope.

Після структурних змін повторіть релевантні unit, integration, API та architecture тести за [правилами тестування](../../../standards/testing-rules.md). Зміни контракту звіряйте з [API-правилами](../../../standards/api-rules.md) і відповідним OpenAPI; зміни persistence — з [правилами БД](../../../standards/database-rules.md).

[Готовий запит до AI](USAGE.md).
