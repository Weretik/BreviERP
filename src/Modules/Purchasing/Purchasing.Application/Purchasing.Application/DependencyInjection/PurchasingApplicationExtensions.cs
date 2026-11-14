using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;

namespace Purchasing.Application.DependencyInjection;

public static class PurchasingApplicationExtensions
{
    /// <summary>Services shared by Host.Api («Оновити зараз», register reads) and Host.Seed (daily sync step).</summary>
    public static IServiceCollection AddPurchasingApplication(this IServiceCollection services)
    {
        services.AddScoped<PurchaseRegisterSync>();
        return services;
    }
}
