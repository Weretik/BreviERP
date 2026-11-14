namespace Warehouse.Application.Features.Stock.Queries.GetHardwareOrder.Results;

/// <param name="Name">Reference name, or the code when the name is unknown.</param>
/// <param name="NameChecked">False: the reference has no name for the code («Потрібна перевірка назв»).</param>
/// <param name="Note">The note of the composition line, shown as «Варіант / примітка».</param>
/// <param name="Unit">Metre, Piece, Service or Kilogram.</param>
/// <param name="Quantity">Sum of norm × products, to 6 decimals.</param>
/// <param name="DueDate">The nearest due date of the tasks that need it; null when none has a date.</param>
public sealed record HardwareNeedResponse(
    string Code,
    string Name,
    bool NameChecked,
    string? Note,
    string Unit,
    decimal Quantity,
    IReadOnlyList<HardwareNeedOrderResponse> Orders,
    DateOnly? DueDate = null);
