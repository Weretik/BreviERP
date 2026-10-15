# Структура Infrastructure

## Відповідальність

Infrastructure — шар адаптерів. Він реалізує контракти Application і зберігає та передає дані до технологічно специфічних систем.

## Цільова структура модуля

```text
<Module>.Infrastructure/
├── DataBase/
│   └── <Module>DbContext.cs
├── Configurations/          EF IEntityTypeConfiguration класи
├── Migrations/              міграції EF і знімок моделі
├── Repositories/            реалізації репозиторіїв EF
├── Projections/             побудова моделей читання зі збережених даних
├── Integrations/            адаптери HTTP, черг і сховищ
├── Services/                адаптери сховища, експорту й сповіщень
├── DependencyInjection/     методи розширення Add<Module>...
└── <Module>.Infrastructure.csproj
```

## Візуалізація адаптера

```text
Абстракція Application
          ▲
          │ реалізує
Адаптер Infrastructure
     ├── EF Core DbContext ──→ PostgreSQL
     ├── зовнішній клієнт ────→ сховище / інший сервіс
     └── побудова проєкції ──→ модель читання
```

## Правила

- Конфігурації EF та міграції ніколи не переносяться в Domain або Application.
- Реєструйте адаптери через наявний метод розширення DI модуля.
- Infrastructure може посилатися на Application та Domain; зворотний напрямок заборонено.
- Тримайте відображення зовнішніх даних на межі адаптера та не допускайте всередину згенеровані типи клієнта.
