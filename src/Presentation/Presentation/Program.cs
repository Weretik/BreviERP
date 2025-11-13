using Application.Common.Extensions;
using Infrastructure.Common.Extensions;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase))
{
    Env.TraversePath().Load();
}

var builder = WebApplication.CreateBuilder(args);

//Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Configuration AdminUser
builder.Services.Configure<AdminUserConfig>(
    builder.Configuration.GetSection("Identity:AdminUser"));

// DI: Application + Infrastructure
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

// DI: Fluxor + State services
builder.Services.AddFluxor(opt => opt.ScanAssemblies(typeof(SharedAssemblyMarker).Assembly));
builder.Services.AddScoped<IBurgerMenuStore, BurgerMenuStore>();
//builder.Services.AddScoped<ICatalogStore, CatalogStore>();

// Services
builder.Services.AddLocalization();
builder.Services.AddMudServices();
builder.Services.AddMudTranslations();
builder.Services.AddHealthChecks();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();



var app = builder.Build();


//Migrations & Seeders
await app.UseAppMigrations();
//await app.UseAppSeeders();

app.UseRequestLocalization(new RequestLocalizationOptions()
    .AddSupportedCultures("uk-UA")
    .AddSupportedUICultures("uk-UA")
    .SetDefaultCulture("uk-UA"));

var uk = CultureInfo.GetCultureInfo("uk-UA");
CultureInfo.DefaultThreadCurrentCulture  = uk;
CultureInfo.DefaultThreadCurrentUICulture = uk;

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Presentation.Client._Imports).Assembly);

// HealthChecks endpoint
app.MapHealthChecks("/health");

app.Run();
