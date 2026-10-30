using Ardalis.Result;
using Crm.Application.CustomSewing.Production.Contracts;
using Crm.Domain.CustomSewing.Attachments;

namespace Crm.Application.CustomSewing.Production.Projection;

internal static class ProductionPackageFileProjection
{
    internal static Result<IReadOnlyList<ProductionPackageFile>> Build(IReadOnlyList<ProductionPinnedFile> source)
    {
        if (source.Any(x => x is null) || source.Count(x => x.Production) > 30) return Invalid("production_files_invalid");
        var files = new Dictionary<Guid, ProductionPackageFile>();
        foreach (var file in source.Where(x => x.Production))
        {
            var extension = file.ContentType switch
            {
                "application/pdf" => "pdf", "image/png" => "png", "image/jpeg" => "jpg", _ => null
            };
            if (file.FilePublicId == Guid.Empty || extension is null || file.Length is <= 0 or > CustomPrivateFile.MaximumBytes ||
                file.Sha256 is not { Length: 64 } || file.Sha256.Any(x => !char.IsAsciiHexDigit(x)))
                return Invalid("production_file_metadata_invalid");
            var projected = new ProductionPackageFile(file.FilePublicId, $"files/{file.FilePublicId:D}.{extension}",
                file.ContentType, file.Length, file.Sha256.ToUpperInvariant());
            if (files.TryGetValue(file.FilePublicId, out var previous) && previous != projected)
                return Invalid("production_file_provenance_conflict");
            files[file.FilePublicId] = projected;
        }
        return Result.Success<IReadOnlyList<ProductionPackageFile>>(Array.AsReadOnly(files.Values.OrderBy(x => x.FilePublicId).ToArray()));
    }

    private static Result<IReadOnlyList<ProductionPackageFile>> Invalid(string code) =>
        Result<IReadOnlyList<ProductionPackageFile>>.Invalid(new ValidationError("productionFiles", code));
}
