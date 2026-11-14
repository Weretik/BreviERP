using Content.Application.Abstractions;
using Content.Application.Features.ProductPages.Shared;

namespace Content.Application.Features.ProductPages.Queries.GetContentProducts;

public sealed class GetContentProductsValidator : AbstractValidator<GetContentProductsQuery>
{
    public GetContentProductsValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 10_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, GetContentProductsQuery.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(GetContentProductsQuery.SearchMaxLength);
        RuleFor(x => x.Type).Must(type => type is null || GetContentProductsQuery.Types.Contains(type))
            .WithMessage("Тип: Sewing або Ppe.");
        RuleFor(x => x.SectionId).GreaterThan(0).When(x => x.SectionId is not null);
        RuleFor(x => x.CategoryId).GreaterThanOrEqualTo(GetContentProductsQuery.WithoutCategory)
            .When(x => x.CategoryId is not null);
    }
}
