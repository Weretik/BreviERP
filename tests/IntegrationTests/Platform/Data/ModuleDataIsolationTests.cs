using Host.Api.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Platform.Data;

/// <summary>
/// A module's data model links to another module only by an id value (module rules «Database schema per module»): no
/// entity of another module in its <c>DbContext</c>, no navigation property to one, and no foreign key into another
/// module's table or schema. What the aggregate needs from the other module (a name, a price at that moment) is kept as
/// a snapshot in its own columns. Builds every module <c>DbContext</c> of the API container; needs no database.
/// </summary>
public sealed class ModuleDataIsolationTests
{
    private static readonly string[] SharedNamespaces = ["BuildingBlocks", "Microsoft", "System"];

    [Test]
    public void Each_module_model_holds_only_its_own_entities_and_links_only_inside_the_module()
    {
        using var services = Services();
        using var scope = services.CreateScope();
        var contexts = ContextTypes(services).ToArray();
        Assert.That(contexts, Is.Not.Empty);

        var violations = new List<string>();
        foreach (var contextType in contexts)
        {
            var context = (DbContext)scope.ServiceProvider.GetRequiredService(contextType);
            violations.AddRange(Violations(context, Module(contextType)));
        }

        TestContext.Out.WriteLine(string.Join(Environment.NewLine, violations));
        Assert.That(violations, Is.Empty);
    }

    internal static IEnumerable<string> Violations(DbContext context, string module)
    {
        foreach (var entity in context.Model.GetEntityTypes())
        {
            if (!BelongsTo(entity.ClrType, module))
                yield return $"{module}: {entity.DisplayName()} is an entity of another module";

            foreach (var navigation in entity.GetNavigations().Cast<INavigationBase>().Concat(entity.GetSkipNavigations()))
                if (!BelongsTo(navigation.TargetEntityType.ClrType, module))
                    yield return $"{module}: {entity.DisplayName()}.{navigation.Name} navigates to another module";

            foreach (var key in entity.GetForeignKeys())
                if (!BelongsTo(key.PrincipalEntityType.ClrType, module)
                    || key.PrincipalEntityType.GetSchema() != entity.GetSchema())
                    yield return $"{module}: {entity.DisplayName()} has a foreign key to {key.PrincipalEntityType.DisplayName()} " +
                                 $"in schema {key.PrincipalEntityType.GetSchema() ?? "(default)"}";
        }
    }

    private static bool BelongsTo(Type type, string module)
    {
        var root = type.Namespace?.Split('.')[0];
        return root == module || SharedNamespaces.Contains(root);
    }

    private static string Module(Type contextType) => contextType.Namespace!.Split('.')[0];

    private static IEnumerable<Type> ContextTypes(ServiceProvider services) =>
        services.GetRequiredService<IServiceCollectionAccessor>().Services
            .Select(descriptor => descriptor.ServiceType)
            .Where(type => type.IsSubclassOf(typeof(DbContext)) && !type.IsAbstract)
            .Distinct();

    private static ServiceProvider Services()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=localhost;Database=brevierp_model_only;Username=none;Password=none"
        });
        builder.Services.AddHostServices(builder.Configuration);
        builder.Services.AddSingleton<IServiceCollectionAccessor>(new ServiceCollectionAccessor(builder.Services));
        return builder.Services.BuildServiceProvider();
    }

    private interface IServiceCollectionAccessor
    {
        IServiceCollection Services { get; }
    }

    private sealed record ServiceCollectionAccessor(IServiceCollection Services) : IServiceCollectionAccessor;
}
