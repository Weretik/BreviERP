
using BuildingBlocks.Domain.Results;

namespace Warehouse.Domain.Stock.Errors;

/// <summary>Business rule errors of fabric stock.</summary>
public static class StockErrors
{
    public static Error CannotRevise() =>
        Error.Conflict("warehouse.cannot_revise", "Сторно і сторнований рух не змінюють.");

    public static Error OutOfRange() =>
        Error.Validation("warehouse.meters_out_of_range", "Метраж перевищує допустимий ліміт.", "meters");

    public static Error WrongSign() =>
        Error.Validation("warehouse.meters_wrong_sign", "Метраж не відповідає виду руху.", "meters");
}
