using Identity.Api.Endpoints.AccessRights;
using Identity.Api.Endpoints.CookieAuth;
using Identity.Api.Endpoints.Profile;
using Identity.Api.Endpoints.Session;
using Identity.Api.Endpoints.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Identity.Application;
using Identity.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Identity.Api;
using BuildingBlocks.Application.Validation;

namespace Identity.Api.DependencyInjection;

public static class IdentityModuleExtensions
{
    /// <summary>Registers the Identity module: Application, Infrastructure, and its Contracts implementations.</summary>
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityConfiguration(configuration);
        services.AddIdentityInfrastructure(configuration);
        services.AddModuleValidators(typeof(IdentityApplicationAssemblyMarker).Assembly);
        return services;
    }

    /// <summary>Maps the Identity module's HTTP endpoints.</summary>
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app) =>
        app.MapCookieAuthEndpoints().MapSessionEndpoints().MapProfileEndpoints().MapAccessRightsEndpoints()
            .MapUsersEndpoints();
}
