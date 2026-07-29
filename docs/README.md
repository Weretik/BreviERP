# Документація BreviERP

Цей каталог містить versioned engineering documentation для BreviERP. Backend-документація ведеться як docs-as-code разом зі змінами коду.

## Backend SDD

- [Архітектура](sdd/architecture/README.md) — фактична топологія backend, модулі та межі шарів.
- [Інженерні правила](sdd/standards/README.md) — backend, API, data, security, testing і delivery rules.
- [Експлуатація](sdd/operations/README.md) — локальний запуск, конфігурація, дані, діагностика та jobs.
- [Специфікації](sdd/specs/README.md) — feature-специфікації та SDD templates.

## Product reference

- [Глосарій](product/glossary.md) — сталі бізнес-терміни, спільні для кількох feature.

## Принцип розміщення

Стале правило належить у `sdd/architecture` або `sdd/standards`. Рішення, вимоги, контракти, задачі та докази перевірки конкретної feature належать у її папку в `sdd/specs/`.
