using Reference.Domain.GarmentParts;

namespace Reference.Domain.GarmentPartOperations;

/// <summary>Operation card fields from the Calculator sheet «Операції».</summary>
/// <param name="Code">Sheet ID (OP-001…).</param>
/// <param name="GarmentPartId">«Група деталей».</param>
/// <param name="Element">«Вузол / елемент».</param>
/// <param name="Variant">«Варіант виконання».</param>
/// <param name="Min">«Норма часу, хв»; 0 when <paramref name="MinutesPerProduct"/> is set.</param>
/// <param name="MinutesPerProduct">The time is entered per product (OP-155 «Нестандартна операція»).</param>
/// <param name="Notes">«Примітки до джерела».</param>
public sealed record GarmentPartOperationDetails(
    string? Code,
    GarmentPartId GarmentPartId,
    string? Element,
    string? Variant,
    string Name,
    decimal Min,
    bool MinutesPerProduct,
    bool IsActive,
    string? Notes);
