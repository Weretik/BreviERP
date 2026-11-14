namespace Catalog.Application.Features.Product.Queries.GetProductMediaUsages;

public sealed class GetProductMediaUsagesValidator : AbstractValidator<GetProductMediaUsagesQuery>
{
    public GetProductMediaUsagesValidator() => RuleFor(x => x.MediaFileIds).NotNull();
}
