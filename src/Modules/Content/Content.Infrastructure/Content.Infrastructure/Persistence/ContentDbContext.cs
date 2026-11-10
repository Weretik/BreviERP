using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Microsoft.EntityFrameworkCore;

namespace Content.Infrastructure.Persistence;

public sealed class ContentDbContext(DbContextOptions<ContentDbContext> options) : DbContext(options), IContentDbContext
{
    public const string Schema = "content";

    public DbSet<ProductPage> ProductPages => Set<ProductPage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly);
    }
}
