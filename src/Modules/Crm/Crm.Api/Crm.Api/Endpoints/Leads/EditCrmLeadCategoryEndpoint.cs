using Crm.Application.Features.Leads.Commands.SaveLeadCategory;
using Crm.Application.Features.Leads.Shared;

namespace Crm.Api.Endpoints.Leads;

internal static class EditCrmLeadCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("categories/{code}", HandleAsync).WithName("editCrmLeadCategory");

    private static async Task<IResult> HandleAsync(string code, SaveLeadCategoryInput input,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new SaveLeadCategoryCommand(code, input), cancellationToken)).ToHttpResult();
}
