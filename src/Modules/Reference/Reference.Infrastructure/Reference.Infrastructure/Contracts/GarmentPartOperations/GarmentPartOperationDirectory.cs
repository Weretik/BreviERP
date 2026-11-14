using Mediator;
using Reference.Application.Features.GarmentPartOperation.GetList;
using Reference.Contracts.GarmentPartOperations;

namespace Reference.Infrastructure.Contracts.GarmentPartOperations;

internal sealed class GarmentPartOperationDirectory(ISender sender) : IGarmentPartOperationDirectory
{
    public async Task<IReadOnlyList<GarmentPartOperationInfo>> ListAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGarmentPartOperationsQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference operations could not be read.");
        return result.Value.Select(x => new GarmentPartOperationInfo(x.Id, x.Code, x.Name, x.Variant, x.Min, x.MinutesPerProduct, x.IsActive)).ToArray();
    }
}
