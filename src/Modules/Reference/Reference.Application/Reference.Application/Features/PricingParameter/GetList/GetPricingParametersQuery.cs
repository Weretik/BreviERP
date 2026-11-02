using Reference.Application.Features.PricingParameter.GetList.DTOs;

namespace Reference.Application.Features.PricingParameter.GetList;

/// <summary>All pricing parameters in sheet order; an empty list when there are none.</summary>
public sealed record GetPricingParametersQuery : IQuery<Result<List<PricingParameterRowDTO>>>;
