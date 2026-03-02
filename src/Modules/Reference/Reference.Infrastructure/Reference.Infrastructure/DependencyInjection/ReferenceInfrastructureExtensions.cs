namespace Reference.Infrastructure.DependencyInjection;

public static class ReferenceInfrastructureExtensions
{
    public static IServiceCollection AddReferenceInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReferenceDbContextServices(configuration);
        services.AddReferenceServices(configuration);

        return services;
    }
}
