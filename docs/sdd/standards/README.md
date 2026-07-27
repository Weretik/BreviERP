# Інженерні стандарти

Ці правила застосовуються до кожної нової backend-функціональності. Спочатку прочитайте [поставку функціональності](workflow/feature-delivery.md), потім використовуйте розділи, релевантні зміні.

```text
standards/
├── workflow/     SDD-процес і definition of done
├── design/       розміщення коду та межі відповідальності
├── contracts/    HTTP-контракти, правила validation і Result/error
├── operations/   доступ до даних, безпека та observability
└── quality/      вибір тестів і команди перевірки
```

## За призначенням

- [Workflow: поставка функціональності та SDD](workflow/feature-delivery.md)
- [Design: організація шарів і коду](design/layer-code-organisation.md)
- [Contracts: API та контракти](contracts/api-contracts.md)
- [Contracts: validation, результати та помилки](contracts/validation-results.md)
- [Operations: дані, безпека та observability](operations/data-security-observability.md)
- [Quality: тестування](quality/testing.md)
