using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Domain.Redirects;
using Content.Domain.SiteCategories;
using Content.Domain.SiteSettings;

namespace Content.Application.Abstractions;

/// <summary>Persistence port of the Content module; implemented by ContentDbContext.</summary>
public interface IContentDbContext
{
    DbSet<ProductPage> ProductPages { get; }
    DbSet<SiteCategory> SiteCategories { get; }
    DbSet<SiteCategoryProduct> SiteCategoryProducts { get; }
    DbSet<Article> Articles { get; }
    DbSet<SiteRedirect> SiteRedirects { get; }
    DbSet<SiteSeoSettings> SiteSeoSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
