using Crm.Application.Customers.Persistence;
using Crm.Domain.Customers;

namespace Crm.Application.Customers.Lookup;

/// <summary>
/// Active clients whose ЄДРПОУ/ІПН is one of these codes, merged ones never; for another module that recognises who
/// paid by the code in a bank statement (for example the statement import of «Бухгалтерія»).
/// </summary>
public sealed record GetCustomersByTaxIdsQuery(IReadOnlyCollection<string> TaxIds) : IQuery<IReadOnlyList<CustomerLookup>>;

public sealed class GetCustomersByTaxIdsQueryValidator : AbstractValidator<GetCustomersByTaxIdsQuery>
{
    public GetCustomersByTaxIdsQueryValidator()
    {
        RuleFor(x => x.TaxIds).NotNull().Must(x => x.Count <= 10_000);
        RuleForEach(x => x.TaxIds).NotEmpty().MaximumLength(80);
    }
}

public sealed class GetCustomersByTaxIdsQueryHandler(ICustomerReadRepository repository)
    : IQueryHandler<GetCustomersByTaxIdsQuery, IReadOnlyList<CustomerLookup>>
{
    public async ValueTask<IReadOnlyList<CustomerLookup>> Handle(GetCustomersByTaxIdsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.TaxIds.Count == 0) return [];
        var wanted = query.TaxIds.Select(x => x.Trim()).Distinct().ToList();
        return await repository.ListAsync(new CustomersByTaxIdsSpec(wanted), cancellationToken);
    }
}

internal sealed class CustomersByTaxIdsSpec : Specification<Customer, CustomerLookup>
{
    public CustomersByTaxIdsSpec(List<string> taxIds)
    {
        Query.AsNoTracking()
            .Where(x => x.MergedIntoId == null && x.Active && x.TaxIdentity != null && taxIds.Contains(x.TaxIdentity.Trim()))
            .OrderBy(x => x.DisplayName);
        Query.Select(x => new CustomerLookup(x.PublicId, x.DisplayName, x.TaxIdentity, x.Active));
    }
}
