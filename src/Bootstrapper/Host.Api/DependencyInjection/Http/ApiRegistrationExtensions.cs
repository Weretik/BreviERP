using BuildingBlocks.Api.Errors;



namespace Host.Api.DependencyInjection.Http;

public static class ApiRegistrationsExtensions
{
    public static IServiceCollection AddApiRegistrations(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerDocumentation();
        services.AddOpenApi();

        services.AddHealthChecks();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
