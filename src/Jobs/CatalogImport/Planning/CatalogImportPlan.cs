using CatalogImport.Source;

namespace CatalogImport.Planning;

public sealed record CatalogImportPlan(
    WorkbookSnapshot Snapshot,
    IReadOnlyList<WorkbookRow> Models,
    IReadOnlyList<WorkbookRow> Articles,
    IReadOnlyList<WorkbookRow> Ppe,
    IReadOnlyList<WorkbookRow> Materials,
    IReadOnlyList<WorkbookRow> Operations,
    IReadOnlyList<string> Warnings);
