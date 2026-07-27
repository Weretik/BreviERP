# SDD-шаблони задач

Оберіть папку за типом роботи. Не поєднуйте шаблони з різних папок, якщо батьківська специфікація явно не потребує обох.

```text
_templates/
├── feature/                 звичайна продуктова/backend-функціональність
│   ├── README.md            модель фаз і цільова структура папки функціональності
│   ├── template-feature.md  батьківська специфікація функціональності
│   ├── contracts/
│   │   └── api-contract.md  видимий клієнту API-контракт
│   └── phases/
│       ├── 01-domain.md ... 06-frontend-handoff.md
│       └── 99-custom-phase.md
├── migration/               багатофазна міграція структури/даних/інтеграції
│   ├── template-migration.md
│   └── phases/
└── git/                     лише планування комітів
    └── git-commit-batching.md
```

## Вибір

| Потреба | Почніть із |
| --- | --- |
| Нова команда, запит, endpoint або бізнес-можливість | `feature/README.md`, потім `feature/template-feature.md` |
| API-контракт потребує окремого погодження | `feature/contracts/api-contract.md` |
| Стандартна фаза backend-реалізації | `feature/phases/01-domain.md` до `06-frontend-handoff.md` |
| Додаткова незалежна фаза | `feature/phases/99-custom-phase.md` |
| Перехід даних, модуля, інтеграції або контракту | `migration/template-migration.md` |
| Свідоме групування комітів | `git/git-commit-batching.md` |

Копіюйте шаблони в папку функціональності перед їх редагуванням. Ніколи не редагуйте шаблон як запис про реалізовану функціональність.
