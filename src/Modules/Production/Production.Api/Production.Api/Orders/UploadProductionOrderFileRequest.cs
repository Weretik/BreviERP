using Microsoft.AspNetCore.Http;

namespace Production.Api.Orders;

/// <param name="Kind">Sizes, Application, Extra or Photo.</param>
/// <param name="ItemId">The item the file is for; empty for the whole order.</param>
public sealed record UploadProductionOrderFileRequest(IFormFile File, string Kind, int? ItemId);
