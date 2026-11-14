using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Domain.Redirects;
using Content.Domain.SiteCategories;
using Content.Domain.SiteCompany;
using Content.Domain.SiteGalleries;
using Content.Domain.SitePages;
using Content.Domain.SiteSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Content.Infrastructure.Persistence;

public sealed class ContentDbContext(DbContextOptions<ContentDbContext> options) : DbContext(options), IContentDbContext
{
    public const string Schema = "content";

    public DbSet<ProductPage> ProductPages => Set<ProductPage>();
    public DbSet<SiteCategory> SiteCategories => Set<SiteCategory>();
    public DbSet<SiteCategoryProduct> SiteCategoryProducts => Set<SiteCategoryProduct>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<SiteRedirect> SiteRedirects => Set<SiteRedirect>();
    public DbSet<SiteSeoSettings> SiteSeoSettings => Set<SiteSeoSettings>();
    public DbSet<SitePage> SitePages => Set<SitePage>();
    public DbSet<SiteCompanyInfo> CompanyInfo => Set<SiteCompanyInfo>();
    public DbSet<SiteGallery> SiteGalleries => Set<SiteGallery>();

    public async Task<IContentTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
        new Transaction(await Database.BeginTransactionAsync(cancellationToken));

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly);
    }

    private sealed class Transaction(IDbContextTransaction transaction) : IContentTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken) => transaction.CommitAsync(cancellationToken);

        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
