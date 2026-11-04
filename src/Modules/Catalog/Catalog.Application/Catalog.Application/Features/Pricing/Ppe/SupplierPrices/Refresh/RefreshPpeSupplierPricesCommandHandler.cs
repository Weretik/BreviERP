using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Import;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Running;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Refresh;

public sealed class RefreshPpeSupplierPricesCommandHandler(IPpeSupplierOnlinePriceReader reader, PpePriceImportRunner runner)
    : ICommandHandler<RefreshPpeSupplierPricesCommand, Result<PpePriceImportResult>>
{
    public const string Trigger = "Оновити зараз";

    public async ValueTask<Result<PpePriceImportResult>> Handle(
        RefreshPpeSupplierPricesCommand command, CancellationToken cancellationToken)
    {
        var source = PpePriceSources.Find(command.Key);
        if (source is null) return Result.NotFound();
        if (!source.CanRefresh)
            return Result.Invalid(new ValidationError("key", source.Method == PpePriceSourceMethod.File
                ? $"{source.Title} оновлюється лише файлом прайсу."
                : $"Автоматичне оновлення {source.Title} ще не підключено."));

        return await runner.RunAsync(source, (products, token) => reader.ReadAsync(source, products, token),
            new PpePriceImportRequest(command.DryRun, Trigger, command.Actor, command.ActorName), cancellationToken);
    }
}
