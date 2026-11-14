using Reference.Application.Features.PricingParameter.Shared;

namespace Reference.Application.Features.PricingParameter.Commands.UpdatePricingParameter;

public sealed record UpdatePricingParameterCommandRequest(
    string Group,
    string Name,
    decimal? Value,
    string Unit,
    bool IsCalculated = false,
    string? Comment = null,
    int SortOrder = 0) : IPricingParameterFields;
