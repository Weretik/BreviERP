# Фаза 06: Передача frontend-контракту

**Статус:** заплановано

Frontend використовує upload response `mediaFileId` для майбутнього Product photo payload. Не зберігати secrets або refresh token у payload. UI має показувати progress, validation error, upload failure та можливість видалити неприкріплений файл.

Перед handoff підтвердити: формат error response, політику доступу, cleanup orphaned files і контракт прив’язування `MediaFileId` до Product.
