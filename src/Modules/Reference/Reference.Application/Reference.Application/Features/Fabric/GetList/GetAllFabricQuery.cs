using Reference.Application.Features.Fabric.GetList.DTOs;

namespace Reference.Application.Features.Fabric.GetList;

public record GetAllFabricQuery() : IQuery<Result<List<FabricRowDTO>>> { }
