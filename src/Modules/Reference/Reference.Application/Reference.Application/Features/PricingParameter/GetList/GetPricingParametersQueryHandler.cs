using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.PricingParameter.GetList.DTOs;
using Reference.Domain.PricingParameters;
using PricingParameterEntity = Reference.Domain.PricingParameters.Entities.PricingParameter;

namespace Reference.Application.Features.PricingParameter.GetList;

public sealed class GetPricingParametersQueryHandler(IReferenceReadRepository<PricingParameterEntity> repository)
    : IQueryHandler<GetPricingParametersQuery, Result<List<PricingParameterRowDTO>>>
{
    public async ValueTask<Result<List<PricingParameterRowDTO>>> Handle(
        GetPricingParametersQuery query, CancellationToken cancellationToken)
    {
        var parameters = await repository.ListAsync(cancellationToken);
        var manualValues = parameters
            .Where(x => !x.IsCalculated && x.Value is not null)
            .ToDictionary(x => x.Code, x => x.Value!.Value, StringComparer.Ordinal);

        var rows = parameters
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value)
            .Select(x => new PricingParameterRowDTO(
                x.Id.Value,
                x.Code,
                x.Group,
                x.Name,
                x.IsCalculated ? PricingParameterFormulas.Calculate(x.Code, manualValues) : x.Value,
                x.Unit,
                x.IsCalculated,
                x.UpdatedOn,
                x.Comment,
                x.SortOrder))
            .ToList();

        return Result.Success(rows);
    }
}
