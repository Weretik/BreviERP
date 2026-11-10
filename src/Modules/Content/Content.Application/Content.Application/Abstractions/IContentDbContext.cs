using Content.Domain.ProductPages;

namespace Content.Application.Abstractions;

/// <summary>Persistence port of the Content module; implemented by ContentDbContext.</summary>
public interface IContentDbContext
{
    DbSet<ProductPage> ProductPages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
