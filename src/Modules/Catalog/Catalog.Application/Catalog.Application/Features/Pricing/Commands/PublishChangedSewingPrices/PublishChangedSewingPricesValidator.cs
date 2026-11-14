namespace Catalog.Application.Features.Pricing.Commands.PublishChangedSewingPrices;

public sealed class PublishChangedSewingPricesValidator : AbstractValidator<PublishChangedSewingPricesCommand>
{
    public PublishChangedSewingPricesValidator() => RuleFor(x => x.Actor).NotEmpty().MaximumLength(200);
}
