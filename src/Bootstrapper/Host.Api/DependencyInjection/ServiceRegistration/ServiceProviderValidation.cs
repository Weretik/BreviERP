namespace Host.Api.DependencyInjection.ServiceRegistration;

/// <summary>
/// Container checks for every environment: a Singleton must not hold a Scoped service (ValidateScopes), and every
/// registration must resolve when the host starts (ValidateOnBuild), so a broken registration fails at start, not on a request.
/// </summary>
public static class ServiceProviderValidation
{
    public static void Apply(ServiceProviderOptions options)
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    }
}
