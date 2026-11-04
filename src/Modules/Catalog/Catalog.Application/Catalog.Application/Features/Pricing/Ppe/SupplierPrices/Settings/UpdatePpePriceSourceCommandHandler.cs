using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Settings;

public sealed class UpdatePpePriceSourceCommandHandler(
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
