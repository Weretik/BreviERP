namespace Crm.Application.Features.Customers.Queries.GetCustomers;

public sealed class GetCustomersValidator : AbstractValidator<GetCustomersQuery>
{
    public const int MaxPage = 1_000_000;
    public const int MaxPageSize = 100;
    public const int MaxSearchLength = 300;

    public GetCustomersValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, MaxPage);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(MaxSearchLength);
    }
}
