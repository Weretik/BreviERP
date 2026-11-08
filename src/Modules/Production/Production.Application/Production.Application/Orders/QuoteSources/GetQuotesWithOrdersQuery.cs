namespace Production.Application.Orders.QuoteSources;

/// <summary>Which of these КП have a production order that is not deleted; Crm asks before deleting a КП.</summary>
public sealed record GetQuotesWithOrdersQuery(IReadOnlyCollection<long> QuoteIds) : IQuery<Result<IReadOnlyList<long>>>;

public sealed class GetQuotesWithOrdersQueryValidator : AbstractValidator<GetQuotesWithOrdersQuery>
{
    public GetQuotesWithOrdersQueryValidator() => RuleFor(x => x.QuoteIds).NotNull();
}

public sealed class GetQuotesWithOrdersQueryHandler(Abstractions.IProductionDbContext db)
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
