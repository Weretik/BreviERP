using BuildingBlocks.Api.Outbox;
using System.Globalization;
using BuildingBlocks.Api.TrustedProxy;
using Identity.Api.DependencyInjection;
using Microsoft.AspNetCore.Localization;
using Host.Api.DependencyInjection.Modules;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseDefaultServiceProvider(ServiceProviderValidation.Apply);

builder.AddSerilog();
builder.Services.AddHostServices(builder.Configuration);
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("uk-UA"), new CultureInfo("ru-RU") };
    options.DefaultRequestCulture = new RequestCulture("uk-UA");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

// One global exception handler in every environment: Ukrainian ProblemDetails with traceId, no stack trace to the client.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi().AllowAnonymous();
}
else
{
    app.UseStatusCodePages();
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseTrustedProxyClientIp();
app.UseRequestLocalization();
app.UseCors("Frontend");
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested &&
                                             !context.Response.HasStarted)
    {
        context.Response.StatusCode = 499;
    }
});
app.UseRateLimiter();
app.UseCookieCsrfProtection();
app.UseAuthentication();
app.UseAuthorization();

app.MapModuleEndpoints();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapOutboxDispatch();

app.Run();
