using BuildingBlocks.Infrastructure.Extensions;
using Host.Seed;
using Microsoft.Extensions.Configuration;


var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

builder.Services.AddSeedHostServices(builder.Configuration);

using var host = builder.Build();

try
{
    await host.Services.MigrateAppAsync();
    await host.Services.SeedAppAsync();
    Environment.ExitCode = 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    Environment.ExitCode = 1;
}
finally
{
    await host.StopAsync();
}
