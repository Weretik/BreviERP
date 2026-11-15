using BuildingBlocks.Application;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Platform.Errors;

/// <summary>Only a moved version or a refused duplicate is a race; any other failed save stays an error.</summary>
public sealed class SaveRacesTests
{
    [Test]
    public async Task A_refused_duplicate_is_a_race()
    {
        await using var db = await DatabaseAsync();
        db.Shelves.AddRange(new Shelf { Code = "A" }, new Shelf { Code = "A" });
        var error = await SaveErrorAsync(db);
        Assert.That(error.IsRace(), Is.True);
    }

    [Test]
    public async Task A_moved_version_is_a_race()
    {
        await using var db = await DatabaseAsync();
        var shelf = new Shelf { Code = "A" };
        db.Shelves.Add(shelf);
        await db.SaveChangesAsync();
        await db.Database.ExecuteSqlRawAsync("UPDATE Shelves SET Version = Version + 1");
        shelf.Code = "B";
        shelf.Version++;
        var error = await SaveErrorAsync(db);
        Assert.That(error, Is.InstanceOf<DbUpdateConcurrencyException>());
        Assert.That(error.IsRace(), Is.True);
    }

    [Test]
    public async Task A_broken_reference_is_not_a_race()
    {
        await using var db = await DatabaseAsync();
        db.Rolls.Add(new Roll { ShelfId = 999 });
        var error = await SaveErrorAsync(db);
        Assert.That(error.IsRace(), Is.False);
    }

    private static async Task<DbUpdateException> SaveErrorAsync(DbContext db)
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException error)
        {
            return error;
        }

        throw new AssertionException("The save was expected to fail.");
    }

    private static async Task<RacesContext> DatabaseAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var db = new RacesContext(new DbContextOptionsBuilder<RacesContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class Shelf
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public int Version { get; set; }
    }

    private sealed class Roll
    {
        public int Id { get; set; }
        public int ShelfId { get; set; }
    }

    private sealed class RacesContext(DbContextOptions<RacesContext> options) : DbContext(options)
    {
        public DbSet<Shelf> Shelves => Set<Shelf>();
        public DbSet<Roll> Rolls => Set<Roll>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Shelf>().HasIndex(x => x.Code).IsUnique();
            b.Entity<Shelf>().Property(x => x.Version).IsConcurrencyToken();
            b.Entity<Roll>().HasOne<Shelf>().WithMany().HasForeignKey(x => x.ShelfId);
        }
    }
}
