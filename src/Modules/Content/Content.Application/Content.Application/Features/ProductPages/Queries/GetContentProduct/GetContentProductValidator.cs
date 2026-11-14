using Content.Application.Abstractions;
using Content.Application.Features.ProductPages.Shared;

namespace Content.Application.Features.ProductPages.Queries.GetContentProduct;

public sealed class GetContentProductValidator : AbstractValidator<GetContentProductQuery>
{
    public GetContentProductValidator() => RuleFor(x => x.ProductId).GreaterThan(0);
}
