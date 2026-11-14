using System.Text.Json.Serialization;
using Catalog.Application.Features.Product.Shared.Sewing;
using Catalog.Application.Features.Product.Shared.Sewing.Inputs;
namespace Catalog.Application.Features.Product.Commands.ReplaceSewingComposition;

public sealed record ReplaceSewingCompositionRequest(string? ModelCode, IReadOnlyList<SewingMaterialInput> Materials, IReadOnlyList<SewingOperationInput> Operations, [property: JsonRequired] long ExpectedVersion);

