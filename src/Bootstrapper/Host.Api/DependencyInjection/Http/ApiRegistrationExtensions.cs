using BuildingBlocks.Api.Errors;
using Host.Api.DependencyInjection.Http.OpenApi;



namespace Host.Api.DependencyInjection.Http;

public static class ApiRegistrationsExtensions
{
    public static IServiceCollection AddApiRegistrations(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddApiDocument();

        services.AddHealthEndpoints();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
