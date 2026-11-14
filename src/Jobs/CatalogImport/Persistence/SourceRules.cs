using BuildingBlocks.Domain.Results;

namespace CatalogImport.Persistence;

/// <summary>
/// A workbook row that breaks a domain rule stops the import: the job writes nothing and the report names the row,
/// like every other source problem it finds (<see cref="InvalidDataException"/>).
/// </summary>
internal static class SourceRules
{
    public static T Require<T>(this DomainResult<T> result, string source) =>
        result.IsSuccess ? result.Value : throw new InvalidDataException($"{source}: {result.Error!.Message}");

    public static void Require(this DomainResult result, string source)
    {
        if (result.IsFailure) throw new InvalidDataException($"{source}: {result.Error!.Message}");
    }
}
