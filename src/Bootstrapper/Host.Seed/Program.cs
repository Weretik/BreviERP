using Host.Seed;
using Host.Seed.Jobs;
using Microsoft.Extensions.Configuration;

var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(
    new Microsoft.Extensions.Hosting.HostApplicationBuilderSettings
    {
        Args = args,
        ContentRootPath = AppContext.BaseDirectory
    });

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

builder.Services.AddSeedHostServices(builder.Configuration);

using var host = builder.Build();
try
{
    await SeedJobDispatcher.RunAsync(host.Services, builder.Configuration);
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
