using System.Text.Json.Serialization;

namespace Catalog.Api.Contracts.Products;

public sealed record PercentWriteRequest(
    [property: JsonConverter(typeof(ProductWriteEnumStringJsonConverter))] string Source,
    int? AdditionalReferenceId,
    decimal? CustomPercent);
