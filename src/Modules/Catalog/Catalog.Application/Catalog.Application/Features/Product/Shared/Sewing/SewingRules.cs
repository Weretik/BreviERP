namespace Catalog.Application.Features.Product.Shared.Sewing;

/// <summary>Field rules shared by the sewing composition and variant requests (CAT-015).</summary>
internal static class SewingRules
{
    internal const int MaxProductId = 1_000_000_000;
    internal const int MaxSaleSkuLength = 80;
    internal const int MaxCodeLength = 80;
    internal const int MaxLines = 2_000;
    internal const decimal MaxFabricQuantity = 1_000m;
    private const decimal MaxDecimal = 999_999_999_999.999999m;

    internal static bool ValidProductId(int id) => id is > 0 and <= MaxProductId;

    internal static bool SixDecimals(decimal value) => decimal.Round(value, 6) == value;

    internal static bool PositiveDecimal(decimal value) => value is > 0 and <= MaxDecimal && SixDecimals(value);

    internal static bool ValidFabricQuantity(decimal value) =>
        value is > 0 and <= MaxFabricQuantity && SixDecimals(value);

    /// <summary>Sale SKU, fabric, fabric quantity and expected version of a fabric variant.</summary>
    internal static void AddVariantRules<T>(AbstractValidator<T> validator, Func<T, string?> saleSku,
        Func<T, int> fabricId, Func<T, decimal> fabricQuantity, Func<T, long> expectedVersion)
    {
        validator.RuleFor(x => saleSku(x))
            .Must(sku => !string.IsNullOrWhiteSpace(sku) && sku.Trim().Length <= MaxSaleSkuLength)
            .OverridePropertyName("saleSku")
            .WithMessage($"Вкажіть артикул продажу до {MaxSaleSkuLength} символів.");
        validator.RuleFor(x => fabricId(x)).GreaterThan(0).OverridePropertyName("fabricId")
            .WithMessage("Оберіть тканину.");
        validator.RuleFor(x => fabricQuantity(x)).Must(ValidFabricQuantity).OverridePropertyName("fabricQuantity")
            .WithMessage("Вкажіть кількість метрів більше 0 і до 1000, не більше шести знаків після коми.");
        validator.RuleFor(x => expectedVersion(x)).GreaterThanOrEqualTo(0).OverridePropertyName("expectedVersion")
            .WithMessage("Версія не може бути від'ємною.");
    }
}
