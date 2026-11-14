namespace Production.Application.Features.Orders.Queries.GetQuotesWithOrders;

/// <summary>Which of these КП have a production order that is not deleted; Crm asks before deleting a КП.</summary>
public sealed record GetQuotesWithOrdersQuery(IReadOnlyCollection<long> QuoteIds) : IQuery<Result<IReadOnlyList<long>>>;

public sealed class GetQuotesWithOrdersValidator : AbstractValidator<GetQuotesWithOrdersQuery>
{
    public GetQuotesWithOrdersValidator() => RuleFor(x => x.QuoteIds).NotNull();
}

public sealed class GetQuotesWithOrdersHandler(Abstractions.IProductionDbContext db)
    : IQueryHandler<GetQuotesWithOrdersQuery, Result<IReadOnlyList<long>>>
{
    public async ValueTask<Result<IReadOnlyList<long>>> Handle(GetQuotesWithOrdersQuery query,
        CancellationToken cancellationToken)
    {
        if (query.QuoteIds.Count == 0) return Result.Success<IReadOnlyList<long>>([]);
        var ids = query.QuoteIds.Select(x => (long?)x).ToArray();
        var found = await db.Orders.AsNoTracking()
            .Where(x => x.DeletedAt == null && ids.Contains(x.QuoteId))
            .Select(x => x.QuoteId!.Value).Distinct()
            .ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<long>>(found);
    }
}
