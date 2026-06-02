using Reference.Application.Features.GarmentPartOperation.GetList.DTOs;

namespace Reference.Application.Features.GarmentPartOperation.GetList;

public sealed record GetGarmentPartOperationsQuery : IQuery<Result<List<GarmentPartOperationRowDTO>>>;
