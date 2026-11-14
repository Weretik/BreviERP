using Reference.Application.Features.PricingSettingsImport.Commands.ImportPricingSettings;
using Reference.Application.Features.PricingSettingsImport.Shared;

namespace Reference.Api.Endpoints.PricingParameters.ImportPricingSettings;

/// <summary>Imports both tables of «Налаштування цін» read from the sheet: dryRun returns the preview; otherwise the
/// rows are saved when none has an error.</summary>
internal static class ImportPricingSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("import", HandleAsync).Produces<PricingSettingsImportResult>()
            .RequireAuthorization("access:pricing-parameters");

    private static async Task<IResult> HandleAsync(ImportPricingSettingsRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ImportPricingSettingsCommand(request.Parameters, request.PpeMarkupRules, request.DryRun,
            request.DeleteMissing), cancellationToken)).ToHttpResult();
}

/// <summary>Both tables of the Calculator sheet «Налаштування цін»; dryRun returns the preview without saving.</summary>
public sealed record ImportPricingSettingsRequest(
    IReadOnlyList<PricingParameterImportRow> Parameters,
    IReadOnlyList<PpeMarkupRuleImportRow> PpeMarkupRules,
    bool DryRun = true,
    bool DeleteMissing = false);
