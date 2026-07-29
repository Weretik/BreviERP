# Product page — design: поточна технічна база

## Реалізовано

- Domain: `ProductId`, `ProductSlug`, `ProductType`, `Name`/`RuName`, `ProductPhoto`, `ProductCategoryReference`; create/update, керування категоріями та фото.
- Infrastructure: EF mapping `Products`, owned collections `ProductPhotos` і `ProductCategoryLinks`, FK категорії з `Restrict`.

## Ще не реалізовано

- Application commands/queries для Product;
- API-контракти, controllers і read-model сторінки товару;
- типоспецифічні Sewing/PPE дані, EF configurations та міграція;
- ручна API-перевірка.

## Технічні правила інтеграції з Reference

- Application перевіряє існування `SupplierId`, `FabricId`, `GarmentAccessoryId`, `GarmentPartOperationId` і `AdditionalReferenceId` перед зміною товару; для коефіцієнта також перевіряється одиниця `%`.
- Product зберігає лише IDs довідникових даних і властивості власних зв’язків; не копіює назви, ціни, контакти або довідникові entities.
- Видалення `GarmentPartOperation`, `Fabric` чи `GarmentAccessory` публікує ідемпотентну міжмодульну подію, яка прибирає відповідні Product links. Потрібно погодити delivery, retry й observability.
- Розрахунок Sewing читає актуальну тривалість операції; якщо потрібна історична норма, це окрема технічна зміна.
