using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Api.Access;
using Accounting.Application.Features.Counterparties.Commands.CreateFinanceCounterparty;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Counterparties.CreateFinanceCounterparty;

internal static class CreateFinanceCounterpartyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createFinanceCounterparty")
            .WithSummary("Create finance counterparty")
            .WithDescription("«Додати фінансового» (verified false: waits for «Підтвердити») or «Зберегти як фінансового контрагента» from an operation (verified true). ЄДРПОУ keeps digits, IBAN letters and digits.")
            .Produces<FinanceCounterpartyResponse>();

    private static async Task<IResult> HandleAsync(FinanceCounterpartyInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new CreateFinanceCounterpartyCommand(input.ToFields(), input.Verified, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
