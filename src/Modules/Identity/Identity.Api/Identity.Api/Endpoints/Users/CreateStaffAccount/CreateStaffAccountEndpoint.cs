using Identity.Application.Features.Users.Commands.CreateStaff;
using Identity.Api.Endpoints.Users.Shared;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.Users.CreateStaffAccount;

internal static class CreateStaffAccountEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createStaffAccount").Produces<StaffAccountResponse>();

    private static async Task<IResult> HandleAsync(CreateStaffAccountRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new CreateStaffCommand(request.Email, request.FullName, request.Role, request.Password,
            request.PhoneNumber), cancellationToken)).ToCreatedHttpResult(StaffAccountResponse.From);
}

public sealed record CreateStaffAccountRequest(
    string Email,
    string FullName,
    string Role,
    string Password,
    string? PhoneNumber = null);
