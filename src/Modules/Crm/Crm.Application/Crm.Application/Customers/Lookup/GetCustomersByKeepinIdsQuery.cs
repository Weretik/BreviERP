using Crm.Application.Customers.KeepinCrmSync;
using Crm.Application.Customers.Persistence;
using Crm.Domain.Customers;

namespace Crm.Application.Customers.Lookup;

/// <summary>
/// Clients linked to these KeepinCRM client ids, archived too, merged ones never; for another module that moves old
/// records keyed by KeepinCRM id (for example the finance archive of «Бухгалтерія»).
/// </summary>
public sealed record GetCustomersByKeepinIdsQuery(IReadOnlyCollection<string> KeepinIds)
    : IQuery<IReadOnlyList<KeepinCustomerLookup>>;

public sealed record KeepinCustomerLookup(string KeepinId, Guid PublicId, string DisplayName, string? TaxIdentity,
    bool Active);

public sealed class GetCustomersByKeepinIdsQueryValidator : AbstractValidator<GetCustomersByKeepinIdsQuery>
{
    public GetCustomersByKeepinIdsQueryValidator()
    {
        RuleFor(x => x.KeepinIds).NotNull().Must(x => x.Count <= 5000);
        RuleForEach(x => x.KeepinIds).NotEmpty().MaximumLength(200);
    }
}

public sealed class GetCustomersByKeepinIdsQueryHandler(ICustomerReadRepository repository)
    : IQueryHandler<GetCustomersByKeepinIdsQuery, IReadOnlyList<KeepinCustomerLookup>>
{
    public async ValueTask<IReadOnlyList<KeepinCustomerLookup>> Handle(GetCustomersByKeepinIdsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.KeepinIds.Count == 0) return [];
        var wanted = query.KeepinIds.Distinct().ToList();
        var rows = await repository.ListAsync(new CustomersByKeepinIdsSpec(wanted), cancellationToken);
        // A merged customer may hold several KeepinCRM ids; each wanted id points to one customer.
        return rows
            .SelectMany(x => x.KeepinIds.Where(wanted.Contains)
                .Select(id => new KeepinCustomerLookup(id, x.PublicId, x.DisplayName, x.TaxIdentity, x.Active)))
            .GroupBy(x => x.KeepinId).Select(x => x.First()).ToList();
    }
}

internal sealed record KeepinCustomerRow(Guid PublicId, string DisplayName, string? TaxIdentity, bool Active,
    List<string> KeepinIds);

internal sealed class CustomersByKeepinIdsSpec : Specification<Customer, KeepinCustomerRow>
{
    public CustomersByKeepinIdsSpec(List<string> keepinIds)
    {
        Query.AsNoTracking()
            .Where(x => x.MergedIntoId == null && x.ExternalIdentities.Any(i => i.Source == KeepinCrmClientIdentity.Source &&
                i.Type == KeepinCrmClientIdentity.Type && keepinIds.Contains(i.ExternalId)));
        Query.Select(x => new KeepinCustomerRow(x.PublicId, x.DisplayName, x.TaxIdentity, x.Active,
            x.ExternalIdentities
                .Where(i => i.Source == KeepinCrmClientIdentity.Source && i.Type == KeepinCrmClientIdentity.Type)
                .Select(i => i.ExternalId).ToList()));
    }
}
