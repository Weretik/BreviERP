namespace Purchasing.Application.Register.GetInvoices;

public sealed class GetPurchaseInvoicesQueryValidator : AbstractValidator<GetPurchaseInvoicesQuery>
{
    public GetPurchaseInvoicesQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 10_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, GetPurchaseInvoicesQuery.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(GetPurchaseInvoicesQuery.SearchMaxLength);
        RuleFor(x => x.Filter).IsInEnum();
        RuleFor(x => x.SupplierId).GreaterThan(0).When(x => x.SupplierId is not null);
    }
}
