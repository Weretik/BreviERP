using Reference.Application.Features.PricingParameter.Shared.DTOs;

namespace Reference.Application.Features.PricingParameter.Update.DTOs;

public sealed record UpdatePricingParameterCommandRequest(
    string Group,
    string Name,
    decimal? Value,
    string Unit,
    bool IsCalculated = false,
    string? Comment = null,
    int SortOrder = 0) : IPricingParameterFields;
