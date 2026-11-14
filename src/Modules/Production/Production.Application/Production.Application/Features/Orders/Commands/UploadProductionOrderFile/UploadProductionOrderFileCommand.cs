using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;

namespace Production.Application.Features.Orders.Commands.UploadProductionOrderFile;

/// <summary>
/// Adds a file to the order or one of its items: sizes, application layout, an extra file or a photo. It goes to the
/// CDN folder of the order.
/// </summary>
public sealed record UploadProductionOrderFileCommand(
    ProductionActor Actor, int OrderId, int? ItemId, string Kind, Stream Content, string FileName, string ContentType,
    long SizeInBytes) : ICommand<Result<ProductionOrderResponse>>
{
    /// <summary>The file name without any folder path a browser may send.</summary>
    public string DisplayName => FileName is null ? string.Empty : FileName[(FileName.LastIndexOfAny(['/', '\\']) + 1)..];
}
