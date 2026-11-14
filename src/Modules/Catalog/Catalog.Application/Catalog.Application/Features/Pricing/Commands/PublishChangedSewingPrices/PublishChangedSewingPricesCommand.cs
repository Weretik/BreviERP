using Catalog.Application.Features.Pricing.Shared.Sewing.AutoPublication;

namespace Catalog.Application.Features.Pricing.Commands.PublishChangedSewingPrices;

/// <summary>
/// The nightly safety net of automatic Sewing price publication (CAT-012, brevierp-sync): publishes every Sewing model
/// whose live price differs from the published one.
/// </summary>
public sealed record PublishChangedSewingPricesCommand(string Actor) : ICommand<Result<SewingPublicationReport>>;
