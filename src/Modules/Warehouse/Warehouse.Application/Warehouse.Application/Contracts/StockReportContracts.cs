namespace Warehouse.Application.Contracts;

// ── «План і факт» (TZ §11) ─────────────────────────────────────────────────────────────────────────────────────────

/// <summary>
/// «План і факт основної тканини за моделями»: items whose fabric was issued for all their products, by model, and the
/// total. <see cref="FabricPlanFactRowResponse.Result"/> is Economy, Overrun, OnPlan, NoPlan or NeedsCheck.
/// </summary>
/// <param name="InProgressItems">Items with issues that did not cover all their products yet; left out.</param>
public sealed record FabricPlanFactResponse(
    IReadOnlyList<FabricPlanFactModelResponse> Models,
    FabricPlanFactRowResponse Total,
    int InProgressItems);

/// <summary>Totals of a model, or of the whole report.</summary>
/// <param name="Orders">Unique orders, not fabric lines.</param>
/// <param name="Products">Each item counted once, whatever its colours and layers.</param>
/// <param name="PlanPerProduct">Saved main-fabric plan of the items with a plan over their products.</param>
/// <param name="FactPerProduct">Issued main fabric over the products.</param>
/// <param name="DifferencePerProduct">Fact − plan per product; null when any item cannot be compared.</param>
/// <param name="Difference">Fact − plan in metres; null when any item cannot be compared.</param>
/// <param name="UncomparableItems">Items «Без плану» or «Потрібна перевірка».</param>
public sealed record FabricPlanFactRowResponse(
    int Orders,
    int Products,
    decimal? PlanPerProduct,
    decimal FactPerProduct,
    decimal? DifferencePerProduct,
    decimal? Difference,
    string Result,
    int UncomparableItems);

public sealed record FabricPlanFactModelResponse(
    string ModelName,
    string? ModelCode,
    FabricPlanFactRowResponse Summary,
    IReadOnlyList<FabricPlanFactItemResponse> Items);

/// <summary>An item in the details: its main-fabric plan and fact, and every material by role.</summary>
/// <param name="Reason">Why the item cannot be compared; null when it can.</param>
public sealed record FabricPlanFactItemResponse(
    int OrderId,
    int OrderNumber,
    string ClientName,
    int ItemId,
    int Products,
    decimal? Plan,
    decimal Fact,
    decimal? Difference,
    string Result,
    string? Reason,
    IReadOnlyList<FabricPlanFactMaterialResponse> Materials);

/// <summary>A planned part, or a material issued beyond the plan.</summary>
/// <param name="Layer">Upper, Lining, Insulation, Fleece, or null when the role is unknown.</param>
/// <param name="Fact">Issued metres; null when nothing was issued for a planned part («не зафіксовано»).</param>
/// <param name="Used">The fabrics and colours actually issued.</param>
public sealed record FabricPlanFactMaterialResponse(
    string? Layer,
    string? PlanFabricCode,
    string? PlanFabricName,
    string? PlanColor,
    decimal? Plan,
    decimal? Fact,
    decimal? Difference,
    IReadOnlyList<string> Used);

// ── «Бронь і що замовити» (TZ §12) ─────────────────────────────────────────────────────────────────────────────────

/// <summary>
/// «Бронь і потреба в тканині»: what the plans of open orders still need of each fabric and colour, what the stock holds
/// for them in order of the orders, and what to order. Reads only; nothing is booked.
/// </summary>
public sealed record FabricBookingResponse(
    decimal Reserved,
    decimal Free,
    decimal ToOrder,
    IReadOnlyList<FabricBookingPairResponse> Pairs,
    IReadOnlyList<FabricBookingUnknownResponse> Unknown,
    DateTimeOffset CalculatedAt);

/// <param name="Stock">Physical balance of the pair; may be negative.</param>
/// <param name="Need">What the plans still need.</param>
/// <param name="Shortage">The negative balance already there, as a positive figure.</param>
public sealed record FabricBookingPairResponse(
    string FabricCode,
    string FabricName,
    string Color,
    decimal Stock,
    decimal Need,
    decimal Reserved,
    decimal Free,
    decimal ToOrder,
    decimal Shortage,
    IReadOnlyList<FabricBookingItemResponse> Items);

/// <param name="Layer">Upper, Lining, Insulation or Fleece.</param>
/// <param name="Covered">What issues covered of the plan.</param>
/// <param name="Remaining">Plan − covered, not below zero.</param>
public sealed record FabricBookingItemResponse(
    int OrderId,
    int OrderNumber,
    string ClientName,
    int ItemId,
    string ModelName,
    DateOnly? DueDate,
    string Layer,
    decimal Plan,
    decimal Covered,
    decimal Remaining,
    decimal Reserved,
    decimal ToOrder);

/// <summary>An item whose need is unknown: it is not counted as zero.</summary>
public sealed record FabricBookingUnknownResponse(
    int OrderId,
    int OrderNumber,
    string ClientName,
    int ItemId,
    string ModelName,
    string Reason);

// ── «Вартість складу» (TZ §13) ─────────────────────────────────────────────────────────────────────────────────────

/// <summary>
/// «Вартість тканин на складі»: the positive balance of every pair at today's price of «Тканини» (the lowest positive
/// price of the fabric group). Not FIFO and not the purchase cost. Shortages are listed apart and do not lower it.
/// </summary>
/// <param name="Value">Sum of the priced pairs, UAH.</param>
/// <param name="IsPartial">Some positive pairs have no price: <see cref="Value"/> is of the priced part only.</param>
public sealed record FabricValuationResponse(
    decimal Value,
    decimal PositiveMeters,
    int UnpricedPairs,
    decimal UnpricedMeters,
    bool IsPartial,
    decimal ShortageMeters,
    decimal ShortageValue,
    IReadOnlyList<FabricValuationFabricResponse> Fabrics,
    IReadOnlyList<FabricValuationShortageResponse> Shortages,
    DateTimeOffset CalculatedAt,
    string PriceSource);

/// <param name="Price">UAH per metre; null when the group has no positive price.</param>
/// <param name="PriceFabricCode">The offer the price was taken from.</param>
public sealed record FabricValuationFabricResponse(
    string FabricCode,
    string FabricName,
    decimal? Price,
    string? PriceFabricCode,
    decimal Meters,
    decimal UnpricedMeters,
    decimal Value,
    IReadOnlyList<FabricValuationColorResponse> Colors);

public sealed record FabricValuationColorResponse(string Color, decimal Meters, decimal? Price, decimal? Value);

/// <param name="Meters">The shortage as a positive figure.</param>
public sealed record FabricValuationShortageResponse(
    string FabricCode,
    string FabricName,
    string Color,
    decimal Meters,
    decimal? Price,
    decimal? Value);

// ── «Фурнітура до замовлення» (TZ §14) ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// «Фурнітура до замовлення»: the full need of physical accessories of every open order, by the composition of the
/// models. Stock and issues are not taken off.
/// </summary>
/// <param name="Orders">Open orders.</param>
/// <param name="Items">Their items (models).</param>
/// <param name="Incomplete">Lines whose quantity is unknown: «Потреба неповна».</param>
public sealed record HardwareOrderResponse(
    int Orders,
    int Items,
    IReadOnlyList<HardwareNeedResponse> Needs,
    IReadOnlyList<HardwareIncompleteResponse> Incomplete,
    DateTimeOffset CalculatedAt);

/// <param name="Name">Reference name, or the code when the name is unknown.</param>
/// <param name="NameChecked">False: the reference has no name for the code («Потрібна перевірка назв»).</param>
/// <param name="Note">The note of the composition line, shown as «Варіант / примітка».</param>
/// <param name="Unit">Metre, Piece, Service or Kilogram.</param>
/// <param name="Quantity">Sum of norm × products, to 6 decimals.</param>
public sealed record HardwareNeedResponse(
    string Code,
    string Name,
    bool NameChecked,
    string? Note,
    string Unit,
    decimal Quantity,
    IReadOnlyList<HardwareNeedOrderResponse> Orders);

/// <summary>One item's share: norm × products = quantity.</summary>
public sealed record HardwareNeedOrderResponse(
    int OrderId,
    int OrderNumber,
    string ClientName,
    int ItemId,
    string ModelName,
    decimal Norm,
    int Products,
    decimal Quantity);

public sealed record HardwareIncompleteResponse(
    int OrderId,
    int OrderNumber,
    string ClientName,
    int ItemId,
    string ModelName,
    string? Code,
    string? Name,
    string Reason);
