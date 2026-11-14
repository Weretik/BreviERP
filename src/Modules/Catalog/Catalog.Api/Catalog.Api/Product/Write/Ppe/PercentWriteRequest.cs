using System.Text.Json.Serialization;
using Catalog.Api.Product.Write;

namespace Catalog.Api.Product.Write.Ppe;

public sealed record PercentWriteRequest(
    [property: JsonConverter(typeof(ProductWriteEnumStringJsonConverter))] string Source,
    int? AdditionalReferenceId,
    decimal? CustomPercent);
