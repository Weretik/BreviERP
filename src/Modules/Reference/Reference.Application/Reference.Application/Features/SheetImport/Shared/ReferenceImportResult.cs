namespace Reference.Application.Features.SheetImport.Shared;

/// <summary>One sheet row of a reference import: what happens (or happened) to it.</summary>
/// <param name="Action">create, update, unchanged or error.</param>
/// <param name="Changes">Ukrainian labels of the fields that differ, for an update.</param>
public sealed record ReferenceImportRowResult(
    int RowNumber,
    string Code,
    string Name,
    string Action,
    IReadOnlyList<string> Changes,
    string? Error);

/// <summary>
/// Preview or outcome of importing a Calculator sheet. <paramref name="Applied"/> is false for a preview and
/// whenever a row has an error: then nothing is saved.
/// </summary>
public sealed record ReferenceImportResult(
    bool Applied,
    int Created,
    int Updated,
    int Unchanged,
    int Errors,
    IReadOnlyList<string> NewSuppliers,
    IReadOnlyList<string> Deactivated,
    IReadOnlyList<ReferenceImportRowResult> Rows);

public static class ReferenceImportActions
{
    public const string Create = "create";
    public const string Update = "update";
    public const string Unchanged = "unchanged";
    public const string Error = "error";
}
