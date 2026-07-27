# Фонові задачі та runbook-и

У поточній перевіреній структурі BreviERP немає окремого console-хоста для імпорту, внутрішнього scheduler-а, черги, retry/backoff, distributed locking або recurring execution. `Host.Api` реєструє наявні background-job services через композицію хоста, але документація не може вигадувати команди чи процедури для не реалізованої задачі.

```text
оператор / cron / CI → погоджена реалізація задачі → scoped module service → PostgreSQL / зовнішній адаптер
```

- [Стан CLI задач](host-jobs-cli.md)
- [Усунення проблем](troubleshooting.md)

Для нової фонової операції спочатку створіть SDD-специфікацію. Вона має визначати команду запуску, контракт, ідемпотентність, timeout/retry-поведінку, locking, observability, безпечний повтор і rollback. Ніколи не вставляйте секрети або справжні endpoint URL у output, issue чи документацію.
