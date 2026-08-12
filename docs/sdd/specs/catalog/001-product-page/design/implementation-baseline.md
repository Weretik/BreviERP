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

- Application перевіряє існування `SupplierId`, `FabricId`, `GarmentAccessoryId`, `GarmentPartOperationId` і вибраних `AdditionalReferenceId` для роздрібного/оптового відсотків перед зміною товару; для кожного такого значення також перевіряється одиниця `%`.
- Product зберігає лише IDs довідникових даних і властивості власних зв’язків; не копіює назви, ціни, контакти або довідникові entities.
- Видалення `GarmentPartOperation`, `Fabric` чи `GarmentAccessory` відхиляється, якщо сутність використовується хоча б одним Product. Зв’язки Product не очищуються автоматично й міжмодульні події для цього не публікуються.
- Видалення `MediaFile` відхиляється, якщо він прив’язаний до Product як фото. Спочатку посилання потрібно прибрати з Product.
- Розрахунок Sewing читає актуальну тривалість операції; якщо потрібна історична норма, це окрема технічна зміна.
