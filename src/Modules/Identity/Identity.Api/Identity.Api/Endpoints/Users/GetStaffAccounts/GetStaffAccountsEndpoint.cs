using Identity.Application.Features.Users.Queries.GetStaffList;
using Identity.Api.Endpoints.Users.Shared;

namespace Identity.Api.Endpoints.Users.GetStaffAccounts;

internal static class GetStaffAccountsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getStaffAccounts").Produces<IReadOnlyList<StaffAccountResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        Results.Ok((await sender.Send(new GetStaffListQuery(), cancellationToken)).Value.Select(StaffAccountResponse.From)
            .ToList());
}
