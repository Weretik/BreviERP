using Reference.Application.Features.PricingParameter.Shared.DTOs;
using Reference.Domain.PricingParameters;

namespace Reference.Application.Features.PricingParameter.Shared;

internal static class PricingParameterDetailsFactory
{
    public static PricingParameterDetails From(IPricingParameterFields fields) => new(
        fields.Group, fields.Name, fields.IsCalculated ? null : fields.Value, fields.Unit, fields.IsCalculated,
        fields.Comment, fields.SortOrder);
}
