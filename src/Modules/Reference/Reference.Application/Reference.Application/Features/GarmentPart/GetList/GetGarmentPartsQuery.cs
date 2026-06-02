using Reference.Application.Features.GarmentPart.GetList.DTOs;

namespace Reference.Application.Features.GarmentPart.GetList;

public sealed record GetGarmentPartsQuery : IQuery<Result<List<GarmentPartRowDTO>>>;
