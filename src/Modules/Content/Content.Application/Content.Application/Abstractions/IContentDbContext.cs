using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;

namespace Content.Application.Abstractions;

/// <summary>Persistence port of the Content module; implemented by ContentDbContext.</summary>
public interface IContentDbContext
{
    DbSet<ProductPage> ProductPages { get; }
    DbSet<SiteCategory> SiteCategories { get; }
    DbSet<SiteCategoryProduct> SiteCategoryProducts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
