using Reference.Application.Features.PricingParameter.Shared;

namespace Reference.Application.Features.PricingParameter.Commands.CreatePricingParameter;

public sealed record CreatePricingParameterCommandRequest(
    string Code,
    string Group,
    string Name,
    decimal? Value,
    string Unit,
    bool IsCalculated = false,
    string? Comment = null,
    int SortOrder = 0) : IPricingParameterFields;
