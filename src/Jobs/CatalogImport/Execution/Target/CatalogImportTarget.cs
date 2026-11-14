namespace CatalogImport.Execution.Target;

public sealed record CatalogImportTarget(string Host, string Database, string SourceSha256);
