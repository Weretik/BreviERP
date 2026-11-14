using Production.Application.Features.Staff.Commands.SetWorkshopEmployeeActive;

namespace Production.Api.Endpoints.Staff;

/// <summary>Archives a worker who left, or returns them to the list.</summary>
internal static class SetWorkshopEmployeeActiveEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/active", HandleAsync).WithName("setWorkshopEmployeeActive");

    private static async Task<IResult> HandleAsync(int id, SetWorkshopEmployeeActiveRequest request,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new SetWorkshopEmployeeActiveCommand(id, request.IsActive, user.UserId()),
            cancellationToken)).ToHttpResult();
}

public sealed record SetWorkshopEmployeeActiveRequest(bool IsActive);
