# Фаза 02: Infrastructure та persistence

**Статус:** перевірено

`MediaFileConfiguration` мапить `catalog.MediaFiles`; `StorageKey` має унікальний індекс. `AdmToolsMediaStorageService` реалізує `IMediaStorageService`, отримує token з `AdmToolsStorage` options, завантажує/видаляє файл та повторює запит один раз після `401`.

Ризик: зовнішнє сховище й БД не є однією транзакцією; часткові помилки потребують операційного recovery.
