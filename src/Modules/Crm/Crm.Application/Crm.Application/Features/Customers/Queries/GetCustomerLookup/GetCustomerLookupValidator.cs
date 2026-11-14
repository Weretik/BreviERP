using Crm.Domain.Customers;

namespace Crm.Application.Features.Customers.Queries.GetCustomerLookup;

public sealed class GetCustomerLookupValidator : AbstractValidator<GetCustomerLookupQuery>
{
    public GetCustomerLookupValidator()
    {
        RuleFor(x => x.Search).MaximumLength(300);
        RuleFor(x => x.Ids).Must(x => x is null || x.Count <= 500);
        RuleFor(x => x.Page).InclusiveBetween(1, 1_000_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 500);
    }
}
