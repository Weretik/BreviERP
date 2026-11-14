namespace Catalog.Application.Features.Pricing.Commands.UpdatePpePriceSource;

/// <summary>
/// Saves the discount off the supplier price and, when chosen, the «Постачальники» row of a source.
/// Only an administrator or the director sends it.
/// </summary>
public sealed record UpdatePpePriceSourceCommand(string Key, decimal DiscountPercent, int? SupplierId, string Actor)
    : ICommand<Result>;
