using System.Globalization;
using Host.Api.Security.AnonymousApiAccess;
using Microsoft.AspNetCore.Localization;
using Host.Api.Crm.Files;
using Host.Api.Crm.Documents;
using Host.Api.Crm.Delivery;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();
builder.Services.AddHostServices(builder.Configuration);
builder.Services.AddAnonymousApiAccess(builder.Configuration);
builder.Services.AddCrmPrivateFiles(builder.Configuration, builder.Environment);
builder.Services.AddCrmQuoteDocuments(builder.Configuration, builder.Environment);
builder.Services.AddCrmDeliveryWorker();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("uk-UA"), new CultureInfo("ru-RU") };
    options.DefaultRequestCulture = new RequestCulture("uk-UA");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(opt => { });
    app.UseStatusCodePages();
    app.UseHsts();
}

app.UseApiDocumentation();
app.UseForwardedHeaders();
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
app.UseAuthentication();
app.UseAnonymousApiAccess();
app.UseAuthorization();

app.MapApiControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();
