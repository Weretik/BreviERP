using System.Text.Json.Serialization;

namespace Catalog.Api.Endpoints.Product.Write.Ppe;

public sealed record PercentWriteRequest(
    [property: JsonConverter(typeof(ProductWriteEnumStringJsonConverter))] string Source,
    int? AdditionalReferenceId,
    decimal? CustomPercent);
