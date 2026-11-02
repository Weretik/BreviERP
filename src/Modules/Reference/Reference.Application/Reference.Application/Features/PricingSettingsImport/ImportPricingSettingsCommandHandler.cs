using Reference.Application.Contracts.Catalog;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.PricingSettingsImport.DTOs;
using PpeMarkupRuleEntity = Reference.Domain.PpeMarkupRules.Entities.PpeMarkupRule;
using PricingParameterEntity = Reference.Domain.PricingParameters.Entities.PricingParameter;

namespace Reference.Application.Features.PricingSettingsImport;

/// <summary>
/// Imports the Calculator sheet «Налаштування цін»: parameters matched by code, PPE markup rules matched by name.
/// Row order in the file becomes the display order. A preview changes nothing; an error in any row stops the
/// whole import. With <c>DeleteMissing</c>, Brevi rows that are not in the sheet are deleted.
/// </summary>
public sealed class ImportPricingSettingsCommandHandler(
    IReferenceRepository<PricingParameterEntity> parameterRepository,
    IReferenceRepository<PpeMarkupRuleEntity> ruleRepository,
    IPricingSettingsChangeNotifier pricingSettingsChanged)
    : ICommandHandler<ImportPricingSettingsCommand, Result<PricingSettingsImportResult>>
{
    public async ValueTask<Result<PricingSettingsImportResult>> Handle(
        ImportPricingSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var parameters = PricingParameterImportPlanner.Plan(command.Parameters, await parameterRepository.ListAsync(cancellationToken),
            command.DeleteMissing);
        var rules = PpeMarkupRuleImportPlanner.Plan(command.PpeMarkupRules, await ruleRepository.ListAsync(cancellationToken),
            command.DeleteMissing);
        var applied = !command.DryRun && parameters.Section.Errors == 0 && rules.Section.Errors == 0;

        if (applied)
        {
            parameters.Apply();
            rules.Apply();
            if (parameters.Created.Count > 0)
                await parameterRepository.AddRangeAsync(parameters.Created, cancellationToken);
            if (parameters.Deleted.Count > 0)
                await parameterRepository.DeleteRangeAsync(parameters.Deleted, cancellationToken);
            if (rules.Created.Count > 0)
                await ruleRepository.AddRangeAsync(rules.Created, cancellationToken);
            if (rules.Deleted.Count > 0)
                await ruleRepository.DeleteRangeAsync(rules.Deleted, cancellationToken);
            // Both repositories share one Reference DbContext, so this saves both tables in one transaction.
            await parameterRepository.SaveChangesAsync(cancellationToken);
            await pricingSettingsChanged.PricingSettingsChangedAsync(cancellationToken);
        }

        return Result.Success(new PricingSettingsImportResult(applied, parameters.Section, rules.Section));
    }
}
