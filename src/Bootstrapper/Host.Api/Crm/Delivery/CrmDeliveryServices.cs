namespace Host.Api.Crm.Delivery;

public static class CrmDeliveryServices
{
    public static IServiceCollection AddCrmDeliveryWorker(this IServiceCollection services)
    {
        services.AddHostedService<QuoteDeliveryWorker>();
        return services;
    }
}
