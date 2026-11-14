using System.Globalization;
using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierOffer;
using Catalog.Domain.Products.Ppe;
using Catalog.Application.Abstractions.Pricing.Ppe;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
using BuildingBlocks.Application;

namespace Catalog.Application.Features.Pricing.Commands.UpdatePpeSupplierOffer;

public sealed class UpdatePpeSupplierOfferHandler(
    ICatalogRepository<ProductEntity> products,
    IProductReferenceReader references,
    IPpePricingInputReader inputs,
    IPpeOfferVersionWriter offers,
    TimeProvider timeProvider) : ICommandHandler<UpdatePpeSupplierOfferCommand, Result<PpeSupplierOffer>>
{
    public const string Source = "Картка ЗІЗ";
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public async ValueTask<Result<PpeSupplierOffer>> Handle(
        UpdatePpeSupplierOfferCommand command, CancellationToken cancellationToken)
    {
        var product = await products.FirstOrDefaultAsync(new PpeProductWithProfileSpec(command.ProductId), cancellationToken);
        if (product is null) return Result.NotFound();

        var snapshot = await references.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Suppliers.ContainsKey(command.SupplierId))
            return Result.Invalid(new ValidationError(nameof(command.SupplierId), "Постачальника не знайдено."));
        var ruleCode = string.IsNullOrWhiteSpace(command.RuleCode) ? null : command.RuleCode.Trim();
        if (ruleCode is not null && await inputs.ReadRuleAsync(ruleCode, cancellationToken) is null)
            return Result.Invalid(new ValidationError(nameof(command.RuleCode), "Правило ціноутворення не знайдено."));

        var current = (await inputs.ReadAsync([command.ProductId], cancellationToken)).SingleOrDefault()?.Offer;
        if (current?.Id != command.ExpectedOfferVersionId)
            return Result.Conflict("Ціни товару змінилися. Оновіть картку.");

        var now = timeProvider.GetUtcNow();
        var changed = product.SetPpeSupplier(new PpeSupplierDetails(SupplierReferenceId.Create(command.SupplierId),
            command.SupplierSku, command.SupplierUrl, command.SupplierPrice, command.AllowManualPrice, ruleCode,
            command.Unit), now);
        if (changed.IsFailure) return changed.Error!.ToResult<PpeSupplierOffer>();

        var offerUnchanged = current is null
            ? command.SupplierPrice is null && ruleCode is null
            : current.Values.SupplierCost == command.SupplierPrice && current.Values.RuleCode == ruleCode;
        if (offerUnchanged)
        {
            await products.UpdateAsync(product, cancellationToken);
        }
        else
        {
            // The writer saves the tracked product changes in the same transaction as the new offer version.
            var today = TimeZoneInfo.ConvertTime(now, Kyiv).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            var values = new PpeOfferValues(ruleCode, command.SupplierPrice, current?.Values.ExternalRetail,
                current?.Values.ExternalWholesale, Source, today);
            var appended = await offers.AppendAsync(command.ProductId, command.ExpectedOfferVersionId, Guid.NewGuid(),
                values, command.Actor, cancellationToken);
            if (!appended.IsSuccess)
                return appended.Status == ResultStatus.Conflict
                    ? Result.Conflict("Ціни товару змінилися. Оновіть картку.")
                    : Result.NotFound();
        }

        return Result.Success(await PpeSupplierOfferReader.ReadAsync(product, references, inputs, cancellationToken));
    }
}
