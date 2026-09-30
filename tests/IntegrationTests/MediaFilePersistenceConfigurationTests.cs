using Catalog.Domain.Media.Entities;
using Catalog.Domain.Media.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IntegrationTests;

public sealed class MediaFilePersistenceConfigurationTests
{
    [Test]
    public void New_entity_receives_a_temporary_id_without_violating_the_domain_factory()
    {
        using var db = new CatalogDbContext(
            new DbContextOptionsBuilder<CatalogDbContext>()
                .UseNpgsql("Host=localhost;Database=model_only;Username=test;Password=test")
                .Options);
        var mediaFile = MediaFile.CreatePending(
            "generated.jpg",
            "image/jpeg",
            1,
            "test",
            "catalog",
            "media/generated.jpg");

        Assert.DoesNotThrow(() => db.MediaFiles.Add(mediaFile));

        var idEntry = db.Entry(mediaFile).Property(x => x.Id);
        Assert.Multiple(() =>
        {
            Assert.That(idEntry.IsTemporary, Is.True);
            Assert.That(idEntry.CurrentValue.Value, Is.LessThan(0));
            Assert.That(mediaFile.Id, Is.EqualTo(default(MediaFileId)));
        });
    }

    [Test]
    public void Id_is_generated_by_the_database()
    {
        using var db = new CatalogDbContext(
            new DbContextOptionsBuilder<CatalogDbContext>()
                .UseNpgsql("Host=localhost;Database=model_only;Username=test;Password=test")
                .Options);

        var idProperty = db.Model
            .FindEntityType(typeof(MediaFile))!
            .FindProperty(nameof(MediaFile.Id))!;

        Assert.Multiple(() =>
        {
            Assert.That(idProperty.ValueGenerated, Is.EqualTo(ValueGenerated.OnAdd));
            Assert.That(
                idProperty.FindAnnotation("Npgsql:ValueGenerationStrategy")?.Value,
                Is.EqualTo(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn));
        });
    }
}
