namespace Reference.Infrastructure.DependencyInjection;

public static class ReferenceServicesExtension
{
    public static IServiceCollection AddReferenceServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReferenceKeepinCrmSync(configuration);

        return services;
    }
}
