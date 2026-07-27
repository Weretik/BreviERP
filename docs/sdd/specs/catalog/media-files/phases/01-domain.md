# Фаза 01: Доменна модель та інваріанти

**Статус:** перевірено

`MediaFile` — aggregate root із `MediaFileId`, metadata storage і статусом `PendingUpload`/`Ready`. `CreatePending` створює лише валідний запис; `MarkUploaded` можливий тільки з `PendingUpload` та вимагає public URL і коректних dimensions. `IsReadyForProductUsage()` є доменною межею для Product.

Критерії: ID, ім’я, content type, розмір, provider, bucket і storage key перевіряються в Domain; Domain не залежить від HTTP чи storage adapter.
