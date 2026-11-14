namespace Production.Application.Features.Orders.Commands.ImportProductionProgress.Rows;

/// <param name="Position">The item the file belongs to; null for the whole order.</param>
/// <param name="Kind">Sizes, Application, Extra or Photo.</param>
public sealed record ProductionFileImportRow(
    int? Position,
    string Kind,
    string Name,
    string ContentType,
    long Bytes,
    string Url,
    DateTimeOffset CreatedAt,
    string? CreatedByName);
