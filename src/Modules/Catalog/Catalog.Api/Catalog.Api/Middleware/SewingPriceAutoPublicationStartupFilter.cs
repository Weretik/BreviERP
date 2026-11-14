using System.Security.Claims;
using Catalog.Contracts.Pricing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Catalog.Application.Features.Pricing.Shared.Sewing.AutoPublication;

namespace Catalog.Api.Middleware;

/// <summary>Puts <see cref="SewingPriceAutoPublicationMiddleware"/> around the whole pipeline of the host.</summary>
internal sealed class SewingPriceAutoPublicationStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.UseMiddleware<SewingPriceAutoPublicationMiddleware>();
        next(app);
    };
}
