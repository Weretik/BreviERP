using Catalog.Application.Features.Pricing.Shared.Sewing.AutoPublication;

namespace Catalog.Application.Features.Pricing.Commands.PublishChangedSewingPrices;

public sealed class PublishChangedSewingPricesHandler(SewingPricePublisher publisher)
    : ICommandHandler<PublishChangedSewingPricesCommand, Result<SewingPublicationReport>>
{
    public async ValueTask<Result<SewingPublicationReport>> Handle(PublishChangedSewingPricesCommand command,
        CancellationToken cancellationToken) =>
        Result.Success(await publisher.PublishChangedAsync(null, command.Actor, cancellationToken));
}
