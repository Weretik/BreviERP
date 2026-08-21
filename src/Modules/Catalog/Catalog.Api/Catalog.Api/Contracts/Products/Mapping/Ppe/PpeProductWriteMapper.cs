using Ardalis.Result;
using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Api.Contracts.Products;

internal static class PpeProductWriteMapper
{
    public static bool HasValues(
        int? supplierId,
        decimal? basePrice,
        PercentWriteRequest? retailPercent,
        PercentWriteRequest? wholesalePercent)
        => supplierId.HasValue || basePrice.HasValue || retailPercent is not null || wholesalePercent is not null;

    public static PpeProductWriteMappingResult Map(
        int? supplierId,
        decimal? basePrice,
        PercentWriteRequest? retailPercent,
        PercentWriteRequest? wholesalePercent)
    {
        var retailPercentMapping = MapPercent(retailPercent, "retailPercent.source");
        var wholesalePercentMapping = MapPercent(wholesalePercent, "wholesalePercent.source");
        var validationErrors = new[] { retailPercentMapping.ValidationError, wholesalePercentMapping.ValidationError }
            .OfType<ValidationError>()
            .ToArray();

        return new(
            new PpeProductRequest(supplierId ?? 0, basePrice ?? 0, retailPercentMapping.Value, wholesalePercentMapping.Value),
            validationErrors);
    }

    private static PercentWriteMappingResult MapPercent(PercentWriteRequest? request, string field)
    {
        var source = default(PricePercentSource);
        var isValid = request is not null && TryParseSource(request.Source, out source);
        return new(
            new PercentRequest(source, request?.AdditionalReferenceId, request?.CustomPercent),
            isValid ? null : new ValidationError(field, $"{field} must be either Reference or Custom."));
    }

    private static bool TryParseSource(string source, out PricePercentSource result)
    {
        result = source switch
        {
            "Reference" => PricePercentSource.Reference,
            "Custom" => PricePercentSource.Custom,
            _ => default
        };
        return source is "Reference" or "Custom";
    }

    private sealed record PercentWriteMappingResult(PercentRequest Value, ValidationError? ValidationError);
}
