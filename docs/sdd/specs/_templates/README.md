# SDD templates

Для нової backend-feature або міграції створіть `docs/sdd/specs/<module>/<NNN>-<feature-slug>/` і скопіюйте `feature/`. Не створюйте порожніх документів.

```text
<NNN>-<feature-slug>/
├── README.md                 orchestration feature
├── requirements/             що і навіщо потрібно
├── design/                   як backend реалізує feature
├── data-model.md             сутності, зв’язки, інваріанти
├── contracts/                API та integration contracts
├── tasks/                    фази й атомарні задачі AI
└── checklist/                quality gates
```

`git/git-commit-batching.md` — окремий шаблон тільки для планування комітів. Для міграції використовуйте цей самий шаблон: baseline/rollback — у `design/`, rollout/verification — у `tasks/`.
