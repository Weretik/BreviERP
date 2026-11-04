using Ardalis.Result;
using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;

namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Files;

/// <summary>Picks the reader for the supplier's file format.</summary>
public sealed class PpeSupplierFileReader : IPpeSupplierFileReader
{
    public Task<Result<IReadOnlyList<SupplierFilePrice>>> ReadAsync(
        PpePriceFileKind kind, IReadOnlyList<SupplierPriceFile> files, CancellationToken cancellationToken)
        => kind switch
        {
            PpePriceFileKind.InsightYml => InsightYmlReader.ReadAsync(files, cancellationToken),
            PpePriceFileKind.CemtoPdf => Task.FromResult(CemtoPdfReader.Read(files)),
            _ => Task.FromResult(Result<IReadOnlyList<SupplierFilePrice>>.Invalid(
                new ValidationError("files", "Цей постачальник не оновлюється з файлу.")))
        };
}
