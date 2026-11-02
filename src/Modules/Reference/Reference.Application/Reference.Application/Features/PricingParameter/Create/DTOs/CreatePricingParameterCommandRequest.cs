using Reference.Application.Features.PricingParameter.Shared.DTOs;

namespace Reference.Application.Features.PricingParameter.Create.DTOs;

public sealed record CreatePricingParameterCommandRequest(
    string Code,
    string Group,
    string Name,
    decimal? Value,
    string Unit,
    bool IsCalculated = false,
    string? Comment = null,
    int SortOrder = 0) : IPricingParameterFields;
