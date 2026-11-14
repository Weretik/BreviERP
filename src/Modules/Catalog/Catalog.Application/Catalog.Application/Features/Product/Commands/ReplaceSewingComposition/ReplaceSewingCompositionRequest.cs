using System.Text.Json.Serialization;
using Catalog.Application.Features.Product.Shared.Sewing;
namespace Catalog.Application.Features.Product.Commands.ReplaceSewingComposition;

public sealed record ReplaceSewingCompositionRequest(string? ModelCode, IReadOnlyList<SewingMaterialInput> Materials, IReadOnlyList<SewingOperationInput> Operations, [property: JsonRequired] long ExpectedVersion);

