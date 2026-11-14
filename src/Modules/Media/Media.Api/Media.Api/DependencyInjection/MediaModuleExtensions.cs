using Media.Api.Endpoints.Files;
using Media.Api.Endpoints.Folders;
using BuildingBlocks.Application.DependencyInjection;
using Media.Application;
using Media.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Media.Api.DependencyInjection;

public static class MediaModuleExtensions
{
    /// <summary>Registers the Media module: Application, Infrastructure, and its Contracts implementations.</summary>
    public static IServiceCollection AddMediaModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediaInfrastructure(configuration);
        services.AddModuleValidators(typeof(MediaApplicationAssemblyMarker).Assembly);
        return services;
    }

    /// <summary>Maps the Media module's HTTP endpoints.</summary>
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app) =>
        app.MapFoldersEndpoints().MapFilesEndpoints();
}
