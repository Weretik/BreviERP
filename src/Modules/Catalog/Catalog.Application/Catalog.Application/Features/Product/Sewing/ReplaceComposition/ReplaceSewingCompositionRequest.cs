using System.Text.Json.Serialization;
namespace Catalog.Application.Features.Product.Sewing;

public sealed record ReplaceSewingCompositionRequest(string? ModelCode, IReadOnlyList<SewingMaterialInput> Materials, IReadOnlyList<SewingOperationInput> Operations, [property: JsonRequired] long ExpectedVersion);

