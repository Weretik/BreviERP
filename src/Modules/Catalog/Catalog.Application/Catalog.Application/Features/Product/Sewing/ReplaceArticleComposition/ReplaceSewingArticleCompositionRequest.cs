using System.Text.Json.Serialization;
namespace Catalog.Application.Features.Product.Sewing;

public sealed record ReplaceSewingArticleCompositionRequest(IReadOnlyList<SewingMaterialInput> Materials, [property: JsonRequired] long ExpectedVersion);

