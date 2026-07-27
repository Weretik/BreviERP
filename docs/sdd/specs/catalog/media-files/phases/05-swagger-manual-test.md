# Фаза 05: Ручна Swagger-перевірка

**Статус:** заплановано

Передумови: Development, `https://localhost:7142/swagger`, PostgreSQL і тестові `AdmToolsStorage` secrets.

| Випадок | Очікування |
| --- | --- |
| Валідний PNG/JPEG/WebP | `200`, status у списку `Ready` |
| JPEG payload із `image/png` | `400` signature validation |
| Непідтриманий MIME | `400` |
| Розмір понад ліміт | `400` |
| Delete наявного ID | `204`, файл зникає зі списку |
| Delete невідомого ID | `404` |
