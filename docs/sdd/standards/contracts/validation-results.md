# Валідація, результати та помилки

FluentValidation validators реєструються скануванням Application assembly і запускаються через Mediator validation behavior. Розміщуйте їх у папці `Validators` сценарію.

Перевіряйте форму request-а на межі: required input, ranges, formats, enum values, length і комбінації input. Перевіряйте state/business invariants у Domain або Application use-case logic.

Повертайте Ardalis.Result для очікуваних failures. Зберігайте наявний API error mapping. Не кидайте винятки для звичайного validation або not-found control flow та не розкривайте internal details у responses.
