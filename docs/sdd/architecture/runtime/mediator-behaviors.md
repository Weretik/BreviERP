# Mediator behaviors

Хост реєструє такі pipeline behaviors для всіх mediator requests у точному порядку:

```text
ISender.Send(...)
  │
  ├── 1. RequestLoggingBehavior
  ├── 2. PerformanceBehavior
  ├── 3. ValidationBehavior
  ├── 4. ExceptionBehavior
  ├── 5. DomainEventDispatcherBehavior
  └── Handler
```

## 1. RequestLoggingBehavior

Логує початок, завершення та failure context mediator request-а за наявними structured logging conventions. Це лише observability; він не змінює бізнес-поведінку і не логує чутливий request payload.

## 2. PerformanceBehavior

Вимірює тривалість request-а через налаштовані `PerformanceBehavior` options та видає діагностику для повільних requests. Не змінює семантику query/command.

## 3. ValidationBehavior

Запускає всі FluentValidation validators для request-а перед handler-ом. Для відповідей `Result` або `Result<T>` validation failures стають `Result.Invalid`; handler не викликається. Validators перевіряють input contracts, а не aggregate state rules.

## 4. ExceptionBehavior

Надає усталений шлях обробки винятків навколо request-а. Очікувані бізнес-наслідки все одно представляються через Ardalis.Result, а не через винятки.

## 5. DomainEventDispatcherBehavior

Працює після успішного handler-а. Він отримує накопичені події з `IDomainEventContext`, очищує їх та публікує кожну через `IDomainEventDispatcher`/Mediator. Сутності накопичують події; контролери та handlers не обходять цей життєвий цикл.

## Правила для нового behavior

- Додавайте behavior лише для справді кросфункціональної потреби, що застосовується до кількох сценаріїв.
- Зберігайте порядок свідомо; документуйте будь-яку його зміну в ADR і відповідній специфікації.
- Ніколи не розміщуйте domain policy, endpoint-specific authorization або use-case orchestration у behavior.
