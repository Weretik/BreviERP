using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices.Sources;

namespace Catalog.Application.Features.Pricing.Commands.UpdatePpePriceSource;

public sealed class UpdatePpePriceSourceHandler(
    IPpePriceSourceStore store,
    IProductReferenceReader references,
    TimeProvider timeProvider) : ICommandHandler<UpdatePpePriceSourceCommand, Result>
{
    public async ValueTask<Result> Handle(UpdatePpePriceSourceCommand command, CancellationToken cancellationToken)
    {
        var source = PpePriceSources.Find(command.Key);
        if (source is null) return Result.NotFound();
        if (command.SupplierId is { } supplierId
            && !(await references.GetSnapshotAsync(cancellationToken)).Suppliers.ContainsKey(supplierId))
            return Result.Invalid(new ValidationError(nameof(command.SupplierId), "Постачальника не знайдено."));

        await store.SaveSettingsAsync(new PpePriceSourceSettings(source.Key, command.SupplierId,
            command.DiscountPercent, timeProvider.GetUtcNow(), command.Actor), cancellationToken);
        return Result.Success();
    }
}
