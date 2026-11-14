using Reference.Application.Features.ProductSection.Commands.ImportProductSections;

namespace Reference.Api.ProductSection;

/// <summary>Rows of the Calculator sheet «Типи і розділи»; dryRun returns the preview without saving.</summary>
public sealed record ImportProductSectionsRequest(
    IReadOnlyList<ProductSectionImportRow> Rows,
    bool DryRun = true,
    bool DeleteMissing = false);
