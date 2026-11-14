using Production.Application.Features.Staff.Commands.CreateWorkshopEmployee;
using Production.Application.Features.Staff.Shared;

namespace Production.Api.Endpoints.Staff.CreateWorkshopEmployee;

internal static class CreateWorkshopEmployeeEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createWorkshopEmployee");

    private static async Task<IResult> HandleAsync(WorkshopEmployeeInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new CreateWorkshopEmployeeCommand(input, user.UserId()), cancellationToken)).ToHttpResult();
}
