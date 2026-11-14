using Mediator;
using Reference.Application.Features.PricingParameter.GetList;
using Reference.Contracts.PricingParameters;

namespace Reference.Infrastructure.Contracts.PricingParameters;

internal sealed class PricingParameterDirectory(ISender sender) : IPricingParameterDirectory
{
    public async Task<IReadOnlyList<PricingParameterInfo>> ListAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPricingParametersQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference pricing parameters could not be read.");
        return result.Value.Select(x => new PricingParameterInfo(x.Id, x.Code, x.Name, x.Value, x.Unit, x.UpdatedOn)).ToArray();
    }
}
