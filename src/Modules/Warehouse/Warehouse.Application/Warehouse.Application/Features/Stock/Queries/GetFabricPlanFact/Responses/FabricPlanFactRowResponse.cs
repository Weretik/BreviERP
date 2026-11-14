namespace Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact.Responses;

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
