using BuildingBlocks.Domain.Errors;

namespace Catalog.Domain.Products.Errors;

public static class ProductErrors
{
    public static Error IdIsRequired() =>
        new("Catalog.Product.Id.Required", "Не вказано товар.");

    public static Error NameIsRequired() =>
        new("Catalog.Product.Name.Required", "Вкажіть назву товару.");

    public static Error NameLengthInvalid(int maxLength) =>
        new("Catalog.Product.Name.LengthInvalid", $"Назва товару — не більше {maxLength} символів.");

    public static Error SlugIsRequired() =>
        new("Catalog.Product.Slug.Required", "Вкажіть адресу (slug) товару.");

    public static Error SlugFormatInvalid() =>
        new("Catalog.Product.Slug.FormatInvalid", "Адреса (slug) товару — лише малі латинські літери, цифри та дефіси.");

    public static Error TypeIsRequired() =>
        new("Catalog.Product.Type.Required", "Вкажіть тип товару.");

    public static Error CategoryIdIsRequired() =>
        new("Catalog.Product.Category.Id.Required", "Не вказано категорію товару.");

    public static Error CategoryAlreadyAttached(int categoryId) =>
        new("Catalog.Product.Category.AlreadyAttached", $"Цю категорію вже додано до товару.", ErrorType.Conflict);

    public static Error CategoryNotFound(int categoryId) =>
        new("Catalog.Product.Category.NotFound", $"Категорію товару не знайдено.", ErrorType.NotFound);

    public static Error PhotoIdIsRequired() =>
        new("Catalog.Product.Photo.Id.Required", "Не вказано фото товару.");

    public static Error PhotoMediaFileIdIsRequired() =>
        new("Catalog.Product.Photo.MediaFileId.Required", "Не вказано файл фото.");

    public static Error PhotoAltLengthInvalid(int maxLength) =>
        new("Catalog.Product.Photo.Alt.LengthInvalid", $"Опис фото — не більше {maxLength} символів.");

    public static Error PhotoSortOrderInvalid() =>
        new("Catalog.Product.Photo.SortOrder.Invalid", "Порядок фото не може бути від'ємним.");

    public static Error PhotoMediaFileAlreadyAttached(int mediaFileId) =>
        new("Catalog.Product.Photo.MediaFile.AlreadyAttached", $"Цей файл уже додано до товару.", ErrorType.Conflict);

    public static Error PhotoAlreadyExists(int photoId) =>
        new("Catalog.Product.Photo.AlreadyExists", $"Це фото вже є в товарі.", ErrorType.Conflict);

    public static Error PhotoNotFound(int photoId) =>
        new("Catalog.Product.Photo.NotFound", $"Фото товару не знайдено.", ErrorType.NotFound);

    public static Error TypeSpecificDataInvalid() =>
        new("Catalog.Product.TypeSpecificData.Invalid", "Дані товару не відповідають його типу.");

    public static Error DetailsBelongToAnotherProduct() =>
        new("Catalog.Product.Details.ProductMismatch", "Ці дані належать іншому товару.");

    public static Error ContentBelongsToAnotherProduct() =>
        new("Catalog.Product.Content.ProductMismatch", "Цей текст належить іншому товару.");

    public static Error CharacteristicRowBelongsToAnotherTable() =>
        new("Catalog.Product.CharacteristicRow.TableMismatch", "Рядок характеристик належить іншій таблиці.");

    public static Error PositiveValueRequired(string field) =>
        new($"Catalog.Product.{field}.PositiveRequired", $"{FieldName(field)}: значення має бути більше нуля.");

    public static Error DuplicateReference(string field, int id) =>
        new($"Catalog.Product.{field}.Duplicate", $"{FieldName(field)}: значення {id} уже додано до товару.", ErrorType.Conflict);

    public static Error TooManyPrimaryFabrics() =>
        new("Catalog.Product.Fabric.Primary.TooMany", "У товару може бути не більше двох основних тканин.");

    public static Error PercentageConfigurationInvalid(string field) =>
        new($"Catalog.Product.{field}.Invalid", $"{FieldName(field)}: оберіть значення з довідника або вкажіть свій відсоток.");

    public static Error DescriptionIsRequired() => new("Catalog.Product.Description.Required", "Вкажіть опис товару.");
    public static Error DescriptionLengthInvalid() => new("Catalog.Product.Description.LengthInvalid", "Опис товару — не більше 20000 символів.");
    public static Error SaleSkuInvalid() => new("Catalog.Product.Article.Sku.Invalid", "Вкажіть артикул продажу до 80 символів.");
    public static Error DuplicateSaleSku(string sku) => new("Catalog.Product.Article.Sku.Duplicate", $"Артикул продажу «{sku}» у цього товару вже є.", ErrorType.Conflict);
    public static Error ArticleLimitReached() => new("Catalog.Product.Article.LimitReached", "У товару може бути не більше 200 варіантів.", ErrorType.Conflict);
    public static Error CompositionParentMismatch() => new("Catalog.Product.Composition.ParentMismatch", "Рядок складу належить іншому товару або варіанту.");
    public static Error CompositionScopeConflict() => new("Catalog.Product.Composition.ScopeConflict", "Спільний рядок складу суперечить рядку окремого варіанта.", ErrorType.Conflict);
    public static Error MaterialReferenceInvalid() => new("Catalog.Product.Material.ReferenceInvalid", "Вид матеріалу, довідник і одиниця не відповідають одне одному.");
    public static Error DuplicateCompositionLine() => new("Catalog.Product.Composition.Line.Duplicate", "У складі є рядок, що повторюється.", ErrorType.Conflict);
    public static Error ArticleLineMustBeFabric() => new("Catalog.Product.Composition.ArticleScope.FabricOnly", "У варіанта може бути лише один рядок тканини.");
    public static Error ArticleNotFound() => new("Catalog.Product.Article.NotFound", "Цей варіант не належить товару.", ErrorType.NotFound);
    public static Error ArticleSpecificOperationsNotSupported() => new("Catalog.Product.Operation.ArticleScope.Unsupported", "Операції для окремого варіанта не підтримуються.");
    public static Error ModelCodeInvalid() => new("Catalog.Product.ModelCode.Invalid", "Код моделі — не більше 80 символів.");
    public static Error ContentTextRequired() => new("Catalog.Product.Content.Text.Required", "Заповніть назви й тексти обома мовами.");
    public static Error ContentTextTooLong(int max) => new("Catalog.Product.Content.Text.LengthInvalid", $"Текст блоку — не більше {max} символів.");
    public static Error OverviewInvalid(string message) => new("Catalog.Product.Overview.Invalid", message);
    public static Error CommentTooLong(int max) => new("Catalog.Product.Composition.CommentTooLong", $"Коментар — не більше {max} символів.");
    public static Error CompositionLineLimitReached() => new("Catalog.Product.Composition.Line.LimitReached", "У складі товару може бути не більше 2000 рядків кожного виду.", ErrorType.Conflict);

    private static string FieldName(string field) => field switch
    {
        "BasePrice" => "Базова ціна",
        "FabricId" => "Тканина",
        "GarmentAccessoryId" => "Фурнітура",
        "GarmentPartOperationId" => "Операція",
        "MetersPerProduct" => "Метрів на виріб",
        "Quantity" => "Кількість",
        "SortOrder" => "Порядок",
        "SupplierId" => "Постачальник",
        "additionalReferenceId" => "Додатковий довідник",
        "customPercent" => "Відсоток",
        _ => "Значення"
    };
}
