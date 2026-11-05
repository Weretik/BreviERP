using Crm.Application.Customers.Persistence;
using Crm.Domain.Customers;

namespace Crm.Application.Customers.Lookup;

/// <summary>
/// Client names for another module that has its own access rule (for example «Бухгалтерія», open to the director only):
/// every client regardless of its manager, merged ones never. <c>Ids</c> picks exact clients (archived too);
/// otherwise <c>Search</c> looks in the name and ЄДРПОУ/ІПН, and <c>ActiveOnly</c> leaves archived ones out.
/// </summary>
public sealed record GetCustomerLookupQuery(string? Search, IReadOnlyCollection<Guid>? Ids, bool ActiveOnly, int Page,
    int PageSize) : IQuery<CustomerLookupPage>;

public sealed record CustomerLookup(Guid PublicId, string DisplayName, string? TaxIdentity, bool Active);

public sealed record CustomerLookupPage(IReadOnlyList<CustomerLookup> Items, int Total);

public sealed class GetCustomerLookupQueryValidator : AbstractValidator<GetCustomerLookupQuery>
{
    public GetCustomerLookupQueryValidator()
    {
        RuleFor(x => x.Search).MaximumLength(300);
        RuleFor(x => x.Ids).Must(x => x is null || x.Count <= 500);
        RuleFor(x => x.Page).InclusiveBetween(1, 1_000_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 500);
    }
}

public sealed class GetCustomerLookupQueryHandler(ICustomerReadRepository repository)
    : IQueryHandler<GetCustomerLookupQuery, CustomerLookupPage>
{
    public async ValueTask<CustomerLookupPage> Handle(GetCustomerLookupQuery query, CancellationToken cancellationToken)
    {
        var total = await repository.CountAsync(new CustomerLookupSpec(query, paged: false), cancellationToken);
        var rows = await repository.ListAsync(new CustomerLookupPageSpec(query), cancellationToken);
        return new CustomerLookupPage(rows, total);
    }
}

internal class CustomerLookupSpec : Specification<Customer>
{
    public CustomerLookupSpec(GetCustomerLookupQuery query, bool paged) => Filter(Query, query, paged);

    internal static void Filter(ISpecificationBuilder<Customer> builder, GetCustomerLookupQuery query, bool paged)
    {
        builder.AsNoTracking().Where(x => x.MergedIntoId == null);
        if (query.Ids is { } ids)
        {
            var wanted = ids.ToList();
            builder.Where(x => wanted.Contains(x.PublicId));
        }
        else
        {
            if (query.ActiveOnly) builder.Where(x => x.Active);
            var term = string.IsNullOrWhiteSpace(query.Search) ? null : CustomerText.Normalize(query.Search);
            if (term != null)
                builder.Where(x => x.NormalizedName.Contains(term)
                    || (x.TaxIdentity != null && x.TaxIdentity.ToUpper().Contains(term)));
        }

        if (!paged) return;
        builder.OrderBy(x => x.DisplayName).ThenBy(x => x.PublicId)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);
    }
}

internal sealed class CustomerLookupPageSpec : Specification<Customer, CustomerLookup>
{
    public CustomerLookupPageSpec(GetCustomerLookupQuery query)
    {
        CustomerLookupSpec.Filter(Query, query, paged: true);
        Query.Select(x => new CustomerLookup(x.PublicId, x.DisplayName, x.TaxIdentity, x.Active));
    }
}
