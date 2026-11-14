namespace Catalog.Application.Features.Product.Queries.GetProductPhotos;

public sealed class GetProductPhotosValidator : AbstractValidator<GetProductPhotosQuery>
{
    public GetProductPhotosValidator() => RuleFor(x => x.ProductId).GreaterThan(0);
}
