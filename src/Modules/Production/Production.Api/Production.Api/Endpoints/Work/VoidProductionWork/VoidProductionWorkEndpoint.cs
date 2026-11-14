using Production.Application.Features.Work.Commands.VoidProductionWork;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Work.VoidProductionWork;

/// <summary>«Сторно» of a record with a reason.</summary>
internal static class VoidProductionWorkEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("work/{workId:int}/void", HandleAsync).WithName("voidProductionWork")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(int workId, VoidProductionWorkInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new VoidProductionWorkCommand(actor, workId, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
