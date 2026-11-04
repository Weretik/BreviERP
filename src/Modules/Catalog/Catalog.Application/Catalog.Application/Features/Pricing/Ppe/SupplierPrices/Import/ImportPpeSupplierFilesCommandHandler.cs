using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Running;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Import;

public sealed class ImportPpeSupplierFilesCommandHandler(IPpeSupplierFileReader fileReader, PpePriceImportRunner runner)
    : ICommandHandler<ImportPpeSupplierFilesCommand, Result<PpePriceImportResult>>
{
    public const string Trigger = "Файл";

    public async ValueTask<Result<PpePriceImportResult>> Handle(
        ImportPpeSupplierFilesCommand command, CancellationToken cancellationToken)
    {
        var source = PpePriceSources.Find(command.Key);
        if (source is null) return Result.NotFound();
        if (!source.AcceptsFiles)
            return Result.Invalid(new ValidationError("files", $"{source.Title} оновлюється без файлів."));
        if (command.Files.Count != source.FileCount)
            return Result.Invalid(new ValidationError("files", source.FileCount == 1
                ? "Додайте один файл прайсу."
                : $"Додайте рівно {source.FileCount} файли прайсу."));

        // Files are read even when no product can change, so a wrong file is reported before the supplier check.
        var filePrices = await fileReader.ReadAsync(source.FileKind, command.Files, cancellationToken);
        if (!filePrices.IsSuccess) return Result.Invalid(filePrices.ValidationErrors.ToList());
        return await runner.RunAsync(source, (_, _) => Task.FromResult(filePrices),
            new PpePriceImportRequest(command.DryRun, Trigger, command.Actor, command.ActorName, FromFiles: true), cancellationToken);
    }
}
