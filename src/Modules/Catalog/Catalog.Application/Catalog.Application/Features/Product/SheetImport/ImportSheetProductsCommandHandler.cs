using Catalog.Application.Contracts.Persistence.SheetImport;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Contracts.SlugGeneration;
using Catalog.Application.Features.Product.SheetImport.DTOs;
using Catalog.Application.Features.Product.SheetImport.Planning;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.SheetImport;

/// <summary>
/// Reads the stored products, the Reference codes, the PPE rules and current offers, plans the import and, when it
/// is not a preview and has no problem, writes it in one transaction. A preview never writes.
/// </summary>
public sealed class ImportSheetProductsCommandHandler(
    ISheetProductImportStore store,
    IProductReferenceCodeReader referenceCodes,
    IPpePricingInputReader pricingInputs,
    IProductSlugGenerator slugGenerator,
    TimeProvider timeProvider) : ICommandHandler<ImportSheetProductsCommand, Result<SheetImportReport>>
{
    public async ValueTask<Result<SheetImportReport>> Handle(
        ImportSheetProductsCommand command, CancellationToken cancellationToken)
    {
        var stored = await store.LoadAsync(cancellationToken);
        var references = await referenceCodes.ReadAsync(cancellationToken);

        var knownRules = new HashSet<string>(StringComparer.Ordinal);
        foreach (var code in command.Ppe.Select(x => x.RuleCode?.Trim()).OfType<string>().Where(x => x.Length > 0).Distinct())
            if (await pricingInputs.ReadRuleAsync(code, cancellationToken) is not null)
                knownRules.Add(code);

        var ppeIds = stored.Products.Where(x => x.Type == ProductType.Ppe).Select(x => x.Id.Value).ToArray();
        var offers = ppeIds.Length == 0
            ? new Dictionary<int, PpeOfferVersion>()
            : (await pricingInputs.ReadAsync(ppeIds, cancellationToken))
                .Where(x => x.Offer is not null).ToDictionary(x => x.ProductId, x => x.Offer!);

        var context = new SheetImportContext(stored.Products, stored.CalculatedProductIds, stored.CalculatedArticleIds,
            references, knownRules, offers, slugGenerator, timeProvider.GetUtcNow());
        var plan = SheetImportPlanner.Plan(command, context);
        if (command.DryRun || plan.Problems.Count > 0)
            return Result.Success(plan.ToReport(false));

        var written = await store.ApplyAsync(plan, command.Actor, cancellationToken);
        return written.IsSuccess
            ? Result.Success(plan.ToReport(true))
            : Result.Conflict(written.Errors.ToArray());
    }
}
