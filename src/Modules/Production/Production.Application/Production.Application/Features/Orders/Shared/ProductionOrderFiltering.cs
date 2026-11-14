using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Domain.FabricPlans;
using Production.Application.Features.Orders.Queries.GetProductionOrders;

namespace Production.Application.Features.Orders.Shared;

/// <summary>Applies «Фільтри» to the orders query. Colour and season are resolved to order ids and model codes first.</summary>
internal static class ProductionOrderFiltering
{
    /// <summary>
    /// Every filter of the list except the status, which the status chips count without. Without
    /// <paramref name="withStage"/> the stage filter is left out too, for the «конвеєр» counts.
    /// </summary>
    public static async Task<IQueryable<ProductionOrder>> FilterAsync(IProductionDbContext db,
        IProductionModelReader models, GetProductionOrdersQuery query, CancellationToken cancellationToken,
        bool withStage = true)
    {
        var orders = db.Orders.AsNoTracking().Where(x => x.DeletedAt == null);
        if (query.CustomerId is { } customerId)
            orders = orders.Where(x => x.CustomerId == customerId);
        if (query.QuoteId is { } quoteId)
            orders = orders.Where(x => x.QuoteId == quoteId);

        if (query.ManagerUserId is { } managerUserId)
            orders = orders.Where(x => x.ManagerUserId == managerUserId);

        if (!string.IsNullOrWhiteSpace(query.Manager))
        {
            var manager = query.Manager.Trim();
            orders = orders.Where(x => x.ManagerName == manager);
        }

        if (withStage && query.Stage is not null)
        {
            var stage = Enum.Parse<ProductionStage>(query.Stage, ignoreCase: true);
            orders = orders.Where(x => x.Items.Min(item => item.CurrentStage) == stage);
        }

        orders = await ApplyAsync(db, models, orders, query.Filters, cancellationToken);

        if (query.DueTo is { } dueTo)
            orders = orders.Where(x => x.Items.Any(item => item.DueDate <= dueTo));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            var number = int.TryParse(search.TrimStart('№', ' '), NumberStyles.None, CultureInfo.InvariantCulture, out var n)
                ? n
                : (int?)null;
            orders = orders.Where(x => x.Number == number ||
                x.ClientName.ToLower().Contains(search) ||
                (x.QuoteNumber != null && x.QuoteNumber.ToLower().Contains(search)) ||
                x.Items.Any(item => item.ModelName.ToLower().Contains(search) ||
                    (item.ModelCode != null && item.ModelCode.ToLower().Contains(search))));
        }

        return orders;
    }

    public static IQueryable<ProductionOrder> ByStatus(IQueryable<ProductionOrder> orders, string? status)
    {
        if (status is null) return orders;
        var parsed = Enum.Parse<ProductionOrderStatus>(status, ignoreCase: true);
        return orders.Where(x => x.Status == parsed);
    }

    private static async Task<IQueryable<ProductionOrder>> ApplyAsync(IProductionDbContext db,
        IProductionModelReader models, IQueryable<ProductionOrder> orders, ProductionOrderFilters? filters,
        CancellationToken cancellationToken)
    {
        if (filters is null) return orders;

        if (!string.IsNullOrWhiteSpace(filters.Client))
        {
            var client = filters.Client.Trim();
            orders = orders.Where(x => x.ClientName == client);
        }

        if (filters.From is { } from) orders = orders.Where(x => x.OrderDate >= from);
        if (filters.To is { } to) orders = orders.Where(x => x.OrderDate <= to);

        if (!string.IsNullOrWhiteSpace(filters.Model))
        {
            var model = filters.Model.Trim().ToUpperInvariant();
            orders = orders.Where(x => x.Items.Any(item => item.ModelCode == model));
        }

        if (!string.IsNullOrWhiteSpace(filters.Fabric))
        {
            var fabric = filters.Fabric.Trim().ToUpperInvariant();
            orders = orders.Where(x => x.Items.Any(item => item.FabricCode == fabric));
        }

        if (filters.HasSeason)
        {
            var season = filters.Season!.Trim();
            var codes = (await models.ListSewingModelsAsync(cancellationToken))
                .Where(x => string.Equals(x.Season, season, StringComparison.CurrentCultureIgnoreCase))
                .Select(x => x.Code.Trim().ToUpperInvariant()).Distinct().ToList();
            orders = orders.Where(x => x.Items.Any(item => item.ModelCode != null && codes.Contains(item.ModelCode)));
        }

        if (filters.HasColor)
        {
            // Additional colours are a JSON column, so the match runs over the loaded items.
            var key = PlanColor.Key(filters.Color!);
            var ids = (await db.Orders.AsNoTracking().Include(x => x.Items).Where(x => x.DeletedAt == null)
                    .ToListAsync(cancellationToken))
                .Where(order => order.Items.Any(item =>
                    item.AdditionalColors.Prepend(item.MainColor).Any(color => PlanColor.Key(color) == key)))
                .Select(order => order.Id).ToList();
            orders = orders.Where(x => ids.Contains(x.Id));
        }

        return orders;
    }
}
