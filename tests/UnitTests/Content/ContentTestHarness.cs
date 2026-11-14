using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Domain.Redirects;
using Content.Domain.SiteCompany;
using Content.Domain.SiteGalleries;
using Content.Domain.SitePages;
using Content.Domain.SiteSettings;
using Content.Domain.SiteCategories;
using Content.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Time.Testing;
using Content.Application.Features.Articles.Commands.DeleteContentArticle;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles;
using Content.Application.Features.Articles.Commands.SaveContentArticle;
using Content.Application.Features.Articles.Commands.SetContentArticleVisibility;
using Content.Application.Features.Articles.Queries.GetContentArticle;
using Content.Application.Features.Articles.Queries.GetContentArticles;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Catalog.Shared;
using Content.Application.Features.ProductPages.Commands.DeleteContentProduct;
using Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;
using Content.Application.Features.ProductPages.Commands.SaveContentProduct;
using Content.Application.Features.ProductPages.Commands.SetContentProductVisibility;
using Content.Application.Features.ProductPages.Queries.GetContentProduct;
using Content.Application.Features.ProductPages.Queries.GetContentProducts;
using Content.Application.Features.Redirects.Commands.DeleteSiteRedirect;
using Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects;
using Content.Application.Features.Redirects.Commands.SaveSiteRedirect;
using Content.Application.Features.Redirects.Queries.GetSiteRedirects;
using Content.Application.Features.Redirects.Shared;
using Content.Application.Features.SeoOverview.Queries.GetSeoOverview;
using Content.Application.Features.SiteCategories.Commands.DeleteSiteCategory;
using Content.Application.Features.SiteCategories.Commands.ImportOldSiteCategories;
using Content.Application.Features.SiteCategories.Commands.ReorderSiteCategories;
using Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;
using Content.Application.Features.SiteCategories.Commands.SetSiteCategoryVisibility;
using Content.Application.Features.SiteCategories.Queries.GetSiteCategories;
using Content.Application.Features.SiteCategories.Queries.GetSiteCategory;
using Content.Application.Features.SiteCompany.Commands.SaveSiteCompany;
using Content.Application.Features.SiteCompany.Queries.GetSiteCompany;
using Content.Application.Features.SiteCompany.Shared;
using Content.Application.Features.SiteGalleries.Commands.SaveSiteGallery;
using Content.Application.Features.SiteGalleries.Commands.TransferSiteGallery;
using Content.Application.Features.SiteGalleries.Queries.GetSiteGallery;
using Content.Application.Features.SiteGalleries.Shared;
using Content.Application.Features.SitePages.Commands.SaveSitePage;
using Content.Application.Features.SitePages.Queries.GetSitePages;
using Content.Application.Features.SitePages.Shared;
using Content.Application.Features.SiteSettings.Commands.SaveSiteSeoSettings;
using Content.Application.Features.SiteSettings.Queries.GetSiteSeoSettings;
using Content.Application.Features.SiteSettings.Shared;

namespace UnitTests.Content;

/// <summary>Catalog products kept in memory instead of the host port over Catalog and Reference.</summary>
internal sealed class FakeCatalogReader : IContentCatalogReader
{
    public List<ContentCatalogProduct> Products { get; } = [];

    public Task<IReadOnlyList<ContentCatalogProduct>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ContentCatalogProduct>>(Products.ToList());

    public Task<ContentCatalogProduct?> GetAsync(int productId, CancellationToken cancellationToken) =>
        Task.FromResult(Products.FirstOrDefault(x => x.Id == productId));

    public List<ContentCatalogSection> Sections { get; } = [];
    public Dictionary<int, List<ContentCatalogPhoto>> Photos { get; } = [];

    public Task<IReadOnlyList<ContentCatalogSection>> GetSectionsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ContentCatalogSection>>(Sections.ToList());

    public Task<IReadOnlyList<ContentCatalogPhoto>> GetPhotosAsync(int productId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ContentCatalogPhoto>>(Photos.GetValueOrDefault(productId) ?? []);
}

/// <summary>Photos of the «Фото» library kept in memory instead of the host port over Media.</summary>
internal sealed class FakeMediaReader : IContentMediaReader
{
    public List<ContentMediaPhoto> Photos { get; } = [];

    public Task<IReadOnlyList<ContentMediaPhoto>> GetAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ContentMediaPhoto>>(Photos.Where(x => ids.Contains(x.Id)).ToList());
}

/// <summary>The old site and the «Фото» library in memory: pictures in <see cref="Broken"/> fail to download.</summary>
internal sealed class FakePhotoImporter(FakeMediaReader media) : IContentPhotoImporter
{
    private int _nextId = 500;
    public Dictionary<string, int> Imported { get; } = new(StringComparer.Ordinal);
    public HashSet<string> Broken { get; } = [];
    public List<(Uri Url, string Key, string Folder)> Downloads { get; } = [];

    public Task<IReadOnlyDictionary<string, int>> FindImportedAsync(IReadOnlyCollection<string> sourceKeys,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<string, int>>(
            Imported.Where(x => sourceKeys.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value));

    public Task<ContentPhotoDownload> DownloadAsync(Uri url, string sourceKey, IReadOnlyList<string> folderPath,
        CancellationToken cancellationToken)
    {
        Downloads.Add((url, sourceKey, string.Join('/', folderPath)));
        if (Broken.Contains(url.AbsoluteUri)) return Task.FromResult(new ContentPhotoDownload(null, "сайт відповів 404"));
        var id = Imported[sourceKey] = _nextId++;
        media.Photos.Add(new ContentMediaPhoto(id, $"https://cdn.test/photos/{id}.jpg", null, 800, 600));
        return Task.FromResult(new ContentPhotoDownload(id, null));
    }
}

/// <summary>The real module DbContext on in-memory SQLite.</summary>
internal sealed class SqliteContentDb(ContentDbContext db) : IContentDbContext
{
    public ContentDbContext Context => db;
    public DbSet<ProductPage> ProductPages => db.ProductPages;
    public DbSet<SiteCategory> SiteCategories => db.SiteCategories;
    public DbSet<SiteCategoryProduct> SiteCategoryProducts => db.SiteCategoryProducts;
    public DbSet<Article> Articles => db.Articles;
    public DbSet<SiteRedirect> SiteRedirects => db.SiteRedirects;
    public DbSet<SiteSeoSettings> SiteSeoSettings => db.SiteSeoSettings;
    public DbSet<SitePage> SitePages => db.SitePages;
    public DbSet<SiteCompanyInfo> CompanyInfo => db.CompanyInfo;
    public DbSet<SiteGallery> SiteGalleries => db.SiteGalleries;
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);

    public async Task<IContentTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
        new Transaction(await db.Database.BeginTransactionAsync(cancellationToken));

    private sealed class Transaction(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction) : IContentTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken) => transaction.CommitAsync(cancellationToken);
        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}

internal sealed class ContentTestHarness : IDisposable
{
    public const string Editor = "admin@brevi.test";
    private readonly SqliteConnection connection;
    private readonly List<ContentDbContext> contexts = [];

    public ContentTestHarness()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var context = Open();
        context.Database.EnsureCreated();
        contexts.Clear();
    }

    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 11, 8, 0, 0, TimeSpan.Zero));
    public FakeCatalogReader Catalog { get; } = new();
    public FakeMediaReader Media { get; } = new();
    public FakePhotoImporter OldSitePhotos => _oldSitePhotos ??= new FakePhotoImporter(Media);
    private FakePhotoImporter? _oldSitePhotos;

    /// <summary>A photo of the library with a small copy.</summary>
    public ContentMediaPhoto AddPhoto(int id, bool withThumbnail = true)
    {
        var photo = new ContentMediaPhoto(id, $"https://cdn.test/photos/{id}.jpg",
            withThumbnail ? $"https://cdn.test/photos/{id}-small.webp" : null, 1200, 800);
        Media.Photos.Add(photo);
        return photo;
    }

    /// <summary>A sewing model with two fabric variants; the name gives the slug <c>kostyum-lider</c>.</summary>
    public ContentCatalogProduct AddSewing(int id = 1, string name = "Костюм Лідер", string code = "MOD-001",
        bool active = true, int? sectionId = 10, string? sectionName = "Костюми")
    {
        var prices = new ContentPriceTiers(1500m, 1400m, 1300m);
        var product = new ContentCatalogProduct(id, "Sewing", code, name, sectionId, sectionName, active,
            "https://brevi.com.ua/kostyum-lider", "https://cdn.test/1.jpg", prices,
            [
                new ContentCatalogVariant(100 + id, $"{code}-A", true, "Грета", "бавовна 35%, щільність 210", prices),
                new ContentCatalogVariant(200 + id, $"{code}-B", true, "Саржа", null, new ContentPriceTiers(1700m, 1600m, 1500m)),
            ]);
        Catalog.Products.Add(product);
        return product;
    }

    public ContentCatalogProduct AddPpe(int id = 50, string name = "Каска будівельна", string code = "ZIZ-001")
    {
        var product = new ContentCatalogProduct(id, "Ppe", code, name, 20, "Захист голови", true, null, null, null, []);
        Catalog.Products.Add(product);
        return product;
    }

    /// <summary>Each handler gets its own context, as each HTTP request does.</summary>
    public SaveContentProductHandler Save() => new(new SqliteContentDb(Open()), Catalog, Time);
    public SetContentProductVisibilityHandler Visibility() => new(new SqliteContentDb(Open()), Catalog, Time);
    public DeleteContentProductHandler Delete() => new(new SqliteContentDb(Open()));
    public GetContentProductHandler One() => new(new SqliteContentDb(Open()), Catalog);
    public ImportOldSiteProductPagesHandler Import() => new(new SqliteContentDb(Open()), Catalog, Time);
    public GetContentProductsHandler List() => new(new SqliteContentDb(Open()), Catalog);

    public SaveSiteCategoryHandler SaveCategory() => new(new SqliteContentDb(Open()), Time);
    public SetSiteCategoryVisibilityHandler CategoryVisibility() => new(new SqliteContentDb(Open()), Time);
    public ReorderSiteCategoriesHandler ReorderCategories() => new(new SqliteContentDb(Open()));
    public DeleteSiteCategoryHandler DeleteCategory() => new(new SqliteContentDb(Open()));
    public ImportOldSiteCategoriesHandler ImportCategories() => new(new SqliteContentDb(Open()), Catalog, Time);
    public GetSiteCategoryHandler Category() => new(new SqliteContentDb(Open()));
    public GetSiteCategoriesHandler Categories() => new(new SqliteContentDb(Open()));

    public SaveContentArticleHandler SaveArticle() => new(new SqliteContentDb(Open()), Media, Time);
    public SetContentArticleVisibilityHandler ArticleVisibility() => new(new SqliteContentDb(Open()), Time);
    public DeleteContentArticleHandler DeleteArticle() => new(new SqliteContentDb(Open()));
    public GetContentArticleHandler Article() => new(new SqliteContentDb(Open()), Media);
    public ImportOldSiteArticlesHandler ImportArticles() => new(new SqliteContentDb(Open()), OldSitePhotos, Time);
    public GetContentArticlesHandler Articles() => new(new SqliteContentDb(Open()), Media);

    public SaveSiteRedirectHandler SaveRedirect() => new(new SqliteContentDb(Open()), Time);
    public DeleteSiteRedirectHandler DeleteRedirect() => new(new SqliteContentDb(Open()));
    public GetSiteRedirectsHandler Redirects() => new(new SqliteContentDb(Open()));
    public ImportOldSiteRedirectsHandler ImportRedirects() => new(new SqliteContentDb(Open()), Catalog, Time);

    public SaveSiteSeoSettingsHandler SaveSeoSettings() => new(new SqliteContentDb(Open()), Media, Time);
    public GetSiteSeoSettingsHandler SeoSettings() => new(new SqliteContentDb(Open()), Media);

    public SaveSitePageHandler SaveSitePage() => new(new SqliteContentDb(Open()), Time);
    public GetSitePagesHandler SitePages() => new(new SqliteContentDb(Open()));
    public GetSeoOverviewHandler SeoOverview() => new(new SqliteContentDb(Open()), Catalog);

    public SaveSiteCompanyHandler SaveCompany() => new(new SqliteContentDb(Open()), Time);
    public GetSiteCompanyHandler Company() => new(new SqliteContentDb(Open()));

    public GetSiteGalleryHandler Gallery() => new(new SqliteContentDb(Open()), Media);
    public SaveSiteGalleryHandler SaveGallery() => new(new SqliteContentDb(Open()), Media, Time);
    public TransferSiteGalleryHandler TransferGallery() => new(new SqliteContentDb(Open()), OldSitePhotos, Media, Time);

    public PublicSiteGalleriesSource PublicGalleries(IMemoryCache? cache = null) =>
        new(new SqliteContentDb(Open()), Media, cache ?? new MemoryCache(new MemoryCacheOptions()));

    public PublicSiteCompanySource PublicCompany(IMemoryCache? cache = null) =>
        new(new SqliteContentDb(Open()), cache ?? new MemoryCache(new MemoryCacheOptions()));

    public PublicSitePagesSource PublicSitePages(IMemoryCache? cache = null) =>
        new(new SqliteContentDb(Open()), cache ?? new MemoryCache(new MemoryCacheOptions()));

    public PublicSiteSettingsSource PublicSiteSettings(IMemoryCache? cache = null) =>
        new(new SqliteContentDb(Open()), Media, cache ?? new MemoryCache(new MemoryCacheOptions()));

    /// <summary>The public redirects as one request sees them; <paramref name="cache"/> is shared to test its lifetime.</summary>
    public PublicRedirectsSource PublicRedirects(IMemoryCache? cache = null) =>
        new(new SqliteContentDb(Open()), cache ?? new MemoryCache(new MemoryCacheOptions()));

    /// <summary>The public catalog as one request sees it; <paramref name="cache"/> is shared to test its lifetime.</summary>
    public PublicCatalogSource Site(IMemoryCache? cache = null) =>
        new(new SqliteContentDb(Open()), Catalog, cache ?? new MemoryCache(new MemoryCacheOptions()));

    /// <summary>The public articles as one request sees them; <paramref name="cache"/> is shared to test its lifetime.</summary>
    public PublicArticlesSource PublicArticles(IMemoryCache? cache = null) =>
        new(new SqliteContentDb(Open()), Media, cache ?? new MemoryCache(new MemoryCacheOptions()));

    public ContentDbContext Open()
    {
        var context = new ContentDbContext(new DbContextOptionsBuilder<ContentDbContext>().UseSqlite(connection).Options);
        contexts.Add(context);
        return context;
    }

    public static ProductPageTextInput Text(string name = "Костюм робочий «Лідер»", string? seoTitle = "Купити костюм Лідер",
        string? seoDescription = "Робочий костюм власного пошиття.") =>
        new(name, seoTitle, seoDescription,
            [new ProductFact("Сезон", "весна, літо, осінь"), new ProductFact(null, "  ")],
            [
                new ProductTextBlock(ProductTextBlockKind.Heading, "Опис", null, null),
                new ProductTextBlock(ProductTextBlockKind.Paragraph, "Зручний  костюм.", "Куртка:", null),
                new ProductTextBlock(ProductTextBlockKind.List, null, null, ["дві кишені", " "]),
            ],
            [new ProductSpecGroup("Куртка", [new ProductSpecRow("Застібка", "блискавка"), new ProductSpecRow("", "x")])]);

    public void Dispose()
    {
        foreach (var context in contexts) context.Dispose();
        connection.Dispose();
    }
}
