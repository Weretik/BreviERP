using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Domain.Redirects;
using Content.Domain.SiteCategories;
using Content.Domain.SiteSettings;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly);
    }
}
