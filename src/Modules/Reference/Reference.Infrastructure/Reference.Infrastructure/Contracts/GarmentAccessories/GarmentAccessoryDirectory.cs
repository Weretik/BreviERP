using Mediator;
using Reference.Contracts.GarmentAccessories;
using Reference.Application.Features.GarmentAccessory.Queries.GetGarmentAccessories;

namespace Reference.Infrastructure.Contracts.GarmentAccessories;

internal sealed class GarmentAccessoryDirectory(ISender sender) : IGarmentAccessoryDirectory
{
    public async Task<IReadOnlyList<GarmentAccessoryInfo>> ListAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGarmentAccessoriesQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference garment accessories could not be read.");
        return result.Value.Select(x => new GarmentAccessoryInfo(x.Id, x.Code, x.Name, x.Unit, x.Price, x.IsActive)).ToArray();
    }
}
