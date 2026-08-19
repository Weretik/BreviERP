using Catalog.Application;

namespace ArchitectureTests;

public sealed class CatalogApplicationDependencyTests
{
    [Test]
    public void Application_does_not_reference_infrastructure_or_entity_framework_core()
    {
        var dependencies = typeof(CatalogApplicationAssemblyMarker).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(dependencies, Does.Not.Contain("Catalog.Infrastructure"));
            Assert.That(dependencies, Does.Not.Contain("Reference.Infrastructure"));
            Assert.That(dependencies, Does.Not.Contain("Microsoft.EntityFrameworkCore"));
        });
    }
}
