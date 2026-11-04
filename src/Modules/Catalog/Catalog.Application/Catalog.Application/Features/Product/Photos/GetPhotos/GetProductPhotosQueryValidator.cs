namespace Catalog.Application.Features.Product.Photos.GetPhotos;

public sealed class GetProductPhotosQueryValidator : AbstractValidator<GetProductPhotosQuery>
{
    public GetProductPhotosQueryValidator() => RuleFor(x => x.ProductId).GreaterThan(0);
}
