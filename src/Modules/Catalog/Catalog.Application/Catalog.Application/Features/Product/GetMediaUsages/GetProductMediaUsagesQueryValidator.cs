namespace Catalog.Application.Features.Product.GetMediaUsages;

public sealed class GetProductMediaUsagesQueryValidator : AbstractValidator<GetProductMediaUsagesQuery>
{
    public GetProductMediaUsagesQueryValidator() => RuleFor(x => x.MediaFileIds).NotNull();
}
