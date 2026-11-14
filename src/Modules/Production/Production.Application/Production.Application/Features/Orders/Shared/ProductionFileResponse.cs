namespace Production.Application.Features.Orders.Shared;

/// <param name="Kind">Sizes, Application, Extra or Photo.</param>
/// <param name="IsOldLink">A Google Drive link moved from the old «Цех»; the file itself is not on the CDN.</param>
public sealed record ProductionFileResponse(
    int Id, int? ItemId, string Kind, string Name, string ContentType, long Bytes, string Url, bool IsOldLink,
    string CreatedByName, DateTimeOffset CreatedAt);
