namespace Crm.Application.Features.Customers.Queries.GetCustomer;

public sealed class GetCustomerValidator : AbstractValidator<GetCustomerQuery>
{
    public GetCustomerValidator() => RuleFor(x => x.CustomerId).NotEmpty();
}
