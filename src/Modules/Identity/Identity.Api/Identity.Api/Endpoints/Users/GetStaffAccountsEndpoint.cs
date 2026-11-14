using Identity.Application.Features.Users.Queries.GetStaffList;
using Identity.Api.Users;

namespace Identity.Api.Endpoints.Users;

internal static class GetStaffAccountsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getStaffAccounts").Produces<IReadOnlyList<StaffAccountResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        Results.Ok((await sender.Send(new GetStaffListQuery(), cancellationToken)).Value.Select(StaffAccountResponse.From)
            .ToList());
}
