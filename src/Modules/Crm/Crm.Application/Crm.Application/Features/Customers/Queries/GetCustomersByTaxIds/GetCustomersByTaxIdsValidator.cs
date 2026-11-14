using Crm.Domain.Customers;
using Crm.Application.Features.Customers.Queries.GetCustomerLookup;

namespace Crm.Application.Features.Customers.Queries.GetCustomersByTaxIds;

public sealed class GetCustomersByTaxIdsValidator : AbstractValidator<GetCustomersByTaxIdsQuery>
{
    public GetCustomersByTaxIdsValidator()
    {
        RuleFor(x => x.TaxIds).NotNull().Must(x => x.Count <= 10_000);
        RuleForEach(x => x.TaxIds).NotEmpty().MaximumLength(80);
    }
}
