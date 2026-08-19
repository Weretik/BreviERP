using System.Globalization;
using System.Resources;

namespace Catalog.Application.Features.Product.Shared.Validation.Resources;

internal static class ProductValidationMessages
{
    private static readonly ResourceManager ResourceManager = new(
        "Catalog.Application.Features.Product.Shared.Validation.Resources.ProductValidationMessages",
        typeof(ProductValidationMessages).Assembly);

    public static string Required(string fieldUk, string fieldRu)
        => Format("Required", Localize(fieldUk, fieldRu));

    public static string Required(string fieldResourceKey)
        => Format("Required", GetString(fieldResourceKey));

    public static string Positive(string fieldUk, string fieldRu)
        => Format("Positive", Localize(fieldUk, fieldRu));

    public static string Positive(string fieldResourceKey)
        => Format("Positive", GetString(fieldResourceKey));

    public static string MaximumLength(string fieldUk, string fieldRu, int maximum)
        => Format("MaximumLength", Localize(fieldUk, fieldRu), maximum);

    public static string MaximumLength(string fieldResourceKey, int maximum)
        => Format("MaximumLength", GetString(fieldResourceKey), maximum);

    public static string Invalid(string fieldUk, string fieldRu)
        => Format("Invalid", Localize(fieldUk, fieldRu));

    public static string Invalid(string fieldResourceKey)
        => Format("Invalid", GetString(fieldResourceKey));

    public static string Duplicate(string collectionUk, string collectionRu)
        => Format("Duplicate", Localize(collectionUk, collectionRu));

    public static string Duplicate(string collectionResourceKey)
        => Format("Duplicate", GetString(collectionResourceKey));

    public static string Localize(string uk, string ru)
        => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ru", StringComparison.OrdinalIgnoreCase)
            ? ru
            : uk;

    private static string Format(string resourceKey, params object[] arguments)
    {
        return string.Format(CultureInfo.CurrentUICulture, GetString(resourceKey), arguments);
    }

    private static string GetString(string resourceKey)
    {
        return ResourceManager.GetString(resourceKey, CultureInfo.CurrentUICulture)
            ?? throw new InvalidOperationException($"Product validation resource '{resourceKey}' is missing.");
    }
}
