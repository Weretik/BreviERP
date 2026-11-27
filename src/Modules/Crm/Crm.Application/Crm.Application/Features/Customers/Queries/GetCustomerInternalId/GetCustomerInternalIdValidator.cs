namespace Crm.Application.Features.Customers.Queries.GetCustomerInternalId;

public sealed class GetCustomerInternalIdValidator : AbstractValidator<GetCustomerInternalIdQuery>
{
    public GetCustomerInternalIdValidator()
    {
        RuleFor(x => x.PublicId).NotEqual(Guid.Empty).WithMessage("Оберіть клієнта.");
    }
}
