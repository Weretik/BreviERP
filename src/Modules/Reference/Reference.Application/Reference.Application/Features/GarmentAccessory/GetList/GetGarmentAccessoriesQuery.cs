using Reference.Application.Features.GarmentAccessory.GetList.DTOs;

namespace Reference.Application.Features.GarmentAccessory.GetList;

public record GetGarmentAccessoriesQuery() : IQuery<Result<List<GarmentAccessoryRowDTO>>> { }
