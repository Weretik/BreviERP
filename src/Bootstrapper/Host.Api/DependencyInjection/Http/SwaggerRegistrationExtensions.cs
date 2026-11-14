
namespace Host.Api.DependencyInjection.Http;

public static class SwaggerRegistrationExtensions
{
    // Development only. Sign in with POST /api/auth/cookie/login on the same host; the browser then sends the
    // session cookie. Changing requests also need the X-CSRF-Token header equal to the brevi.csrf cookie.
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen();
        return services;
    }
}
