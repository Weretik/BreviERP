using CatalogImport.Execution;
using Microsoft.Extensions.Configuration;

namespace Host.Seed.Jobs;

internal static class CatalogImportCommandRunner
{
    public static Task RunAsync(IConfiguration configuration) => CatalogImportCommand.RunAsync(configuration);
}
