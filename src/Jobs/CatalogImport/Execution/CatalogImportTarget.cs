namespace CatalogImport.Execution;

public sealed record CatalogImportTarget(string Host, string Database, string SourceSha256);
