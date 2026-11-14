namespace Production.Application.Features.Orders.Shared.Filtering;

/// <summary>
/// «Фільтри» of «Виробничі завдання». <paramref name="Client"/> is the exact client name; <paramref name="From"/> and
/// <paramref name="To"/> bound the order date; <paramref name="Model"/> and <paramref name="Fabric"/> are codes of an
/// item; <paramref name="Color"/> matches an item's main or additional colour ignoring case and outer spaces;
/// <paramref name="Season"/> is the Catalog season of an item's model.
/// </summary>
public sealed record ProductionOrderFilters(
    string? Client = null, DateOnly? From = null, DateOnly? To = null, string? Model = null, string? Fabric = null,
    string? Color = null, string? Season = null)
{
    public bool HasColor => !string.IsNullOrWhiteSpace(Color);
    public bool HasSeason => !string.IsNullOrWhiteSpace(Season);
}
