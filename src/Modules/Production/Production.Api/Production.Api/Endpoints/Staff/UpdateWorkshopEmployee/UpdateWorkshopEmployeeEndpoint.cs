using Production.Application.Features.Staff.Commands.UpdateWorkshopEmployee;
using Production.Application.Features.Staff.Shared;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Staff.UpdateWorkshopEmployee;

internal static class UpdateWorkshopEmployeeEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync).WithName("updateWorkshopEmployee");

    private static async Task<IResult> HandleAsync(int id, WorkshopEmployeeInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateWorkshopEmployeeCommand(id, input, user.UserId()),
            cancellationToken)).ToHttpResult();
}
