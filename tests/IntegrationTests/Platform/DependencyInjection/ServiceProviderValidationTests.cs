using Host.Seed;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Host.Api.DependencyInjection;

namespace IntegrationTests.Platform.DependencyInjection;

/// <summary>
/// Every host container in Production mode with scope and build validation: no Singleton holds a Scoped service
/// (such as a DbContext) and every registration resolves. Needs no database: nothing connects while the container is built.
/// </summary>
public sealed class ServiceProviderValidationTests
{
    private static readonly Dictionary<string, string?> Settings = new()
    {
        ["ConnectionStrings:Default"] = "Host=localhost;Database=brevierp_validation;Username=none;Password=none"
    };

    [Test]
    public void Host_api_container_is_valid_in_production()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.Configuration.AddInMemoryCollection(Settings);
        builder.Host.UseDefaultServiceProvider(ServiceProviderValidation.Apply);
        builder.Services.AddHostServices(builder.Configuration);

        Assert.DoesNotThrow(() =>
        {
            using var app = builder.Build();
        });
    }

    [Test]
    public void Host_seed_container_is_valid_in_production()
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(
            new HostApplicationBuilderSettings { EnvironmentName = "Production" });
        builder.Configuration.AddInMemoryCollection(Settings);
        builder.Services.AddSeedHostServices(builder.Configuration);
        builder.ConfigureContainer(new DefaultServiceProviderFactory(SeedHostServicesExtensions.ServiceProviderOptions));

        Assert.DoesNotThrow(() =>
        {
            using var host = builder.Build();
        });
    }
}
