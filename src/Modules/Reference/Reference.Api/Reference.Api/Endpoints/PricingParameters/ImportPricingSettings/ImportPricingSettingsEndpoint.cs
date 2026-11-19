using Microsoft.AspNetCore.RateLimiting;
using Reference.Application.Features.PricingSettingsImport.Commands.ImportPricingSettings;
using Reference.Application.Features.PricingSettingsImport.Shared;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.PricingParameters.ImportPricingSettings;

/// <summary>Imports both tables of «Налаштування цін» read from the sheet: dryRun returns the preview; otherwise the
/// rows are saved when none has an error.</summary>
internal static class ImportPricingSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("import", HandleAsync)
            .WithName("importPricingSettings").RequireRateLimiting("HeavyOperation")
            .WithSummary("Import both tables of the Calculator sheet «Налаштування цін»")
            .WithDescription("Requires access:pricing-parameters. Parameters are matched by code, PPE markup rules by name; the row number becomes the display order. dryRun (default) returns the preview only. Otherwise nothing is saved when any row has an error. deleteMissing deletes parameters and rules that are not in the sheet. Repeating the same file changes nothing.")
            .Produces<PricingSettingsImportResult>()
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
