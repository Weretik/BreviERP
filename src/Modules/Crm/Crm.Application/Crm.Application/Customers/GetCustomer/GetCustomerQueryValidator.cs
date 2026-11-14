namespace Crm.Application.Customers.GetCustomer;

public sealed class GetCustomerQueryValidator : AbstractValidator<GetCustomerQuery>
{
    public GetCustomerQueryValidator() => RuleFor(x => x.CustomerId).NotEmpty();
}
