using System.Text.Json.Serialization;

namespace Catalog.Api.Endpoints.Product.Write;

public sealed record PercentWriteRequest(
    [property: JsonConverter(typeof(ProductWriteEnumStringJsonConverter))] string Source,
    int? AdditionalReferenceId,
    decimal? CustomPercent);
