# Фаза 03: Application-сценарії

**Статус:** перевірено

| Операція | Тип | Result |
| --- | --- | --- |
| UploadMediaFile | command | ID, storage key, URL, content type |
| GetMediaFiles | query | list projection |
| DeleteMediaFile | command | status/not found |

`UploadMediaFileCommandValidator` перевіряє request, розмір, MIME type та сигнатуру JPEG/PNG/WebP; validator повертає позицію seekable stream. Upload створює pending record, викликає storage та позначає файл Ready. Read використовує projection. Delete видаляє storage object, потім persistence record.
