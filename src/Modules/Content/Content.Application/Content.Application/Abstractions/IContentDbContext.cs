using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Domain.Redirects;
using Content.Domain.SiteCategories;
using Content.Domain.SiteCompany;
using Content.Domain.SiteGalleries;
using Content.Domain.SitePages;
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
    DbSet<SitePage> SitePages { get; }
    DbSet<SiteCompanyInfo> CompanyInfo { get; }
    DbSet<SiteGallery> SiteGalleries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>One transaction for a use case that must save more than once (new ids before their links).</summary>
    Task<IContentTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}

public interface IContentTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
