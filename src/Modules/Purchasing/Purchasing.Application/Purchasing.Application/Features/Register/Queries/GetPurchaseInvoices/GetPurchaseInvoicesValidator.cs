namespace Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices;

public sealed class GetPurchaseInvoicesValidator : AbstractValidator<GetPurchaseInvoicesQuery>
{
    public GetPurchaseInvoicesValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 10_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, GetPurchaseInvoicesQuery.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(GetPurchaseInvoicesQuery.SearchMaxLength);
        RuleFor(x => x.Filter).IsInEnum();
        RuleFor(x => x.SupplierId).GreaterThan(0).When(x => x.SupplierId is not null);
        RuleFor(x => x.Vat).IsInEnum().When(x => x.Vat is not null);
        RuleFor(x => x.DocumentTo).GreaterThanOrEqualTo(x => x.DocumentFrom)
            .When(x => x.DocumentFrom is not null && x.DocumentTo is not null)
            .WithMessage("Дата «по» раніша за дату «з».");
    }
}
