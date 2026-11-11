using Content.Application.Abstractions;
using Content.Application.ProductPages.Delete;
using Content.Application.ProductPages.GetList;
using Content.Application.ProductPages.GetOne;
using Content.Application.ProductPages.ImportOldSite;
using Content.Application.ProductPages.Save;
using Content.Application.ProductPages.SetVisibility;
using Content.Application.PublicCatalog;
using Content.Application.SiteCategories.Delete;
using Content.Application.SiteCategories.GetList;
using Content.Application.SiteCategories.GetOne;
using Content.Application.SiteCategories.ImportOldSite;
using Content.Application.SiteCategories.Reorder;
using Content.Application.SiteCategories.Save;
using Content.Application.SiteCategories.SetVisibility;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;
using Content.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Time.Testing;

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

/// <summary>The real module DbContext on in-memory SQLite.</summary>
internal sealed class SqliteContentDb(ContentDbContext db) : IContentDbContext
{
    public ContentDbContext Context => db;
    public DbSet<ProductPage> ProductPages => db.ProductPages;
    public DbSet<SiteCategory> SiteCategories => db.SiteCategories;
    public DbSet<SiteCategoryProduct> SiteCategoryProducts => db.SiteCategoryProducts;
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
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
    public SaveContentProductCommandHandler Save() => new(new SqliteContentDb(Open()), Catalog, Time);
    public SetContentProductVisibilityCommandHandler Visibility() => new(new SqliteContentDb(Open()), Catalog, Time);
    public DeleteContentProductCommandHandler Delete() => new(new SqliteContentDb(Open()));
    public GetContentProductQueryHandler One() => new(new SqliteContentDb(Open()), Catalog);
    public ImportOldSiteProductPagesCommandHandler Import() => new(new SqliteContentDb(Open()), Catalog, Time);
    public GetContentProductsQueryHandler List() => new(new SqliteContentDb(Open()), Catalog);

    public SaveSiteCategoryCommandHandler SaveCategory() => new(new SqliteContentDb(Open()), Time);
    public SetSiteCategoryVisibilityCommandHandler CategoryVisibility() => new(new SqliteContentDb(Open()), Time);
    public ReorderSiteCategoriesCommandHandler ReorderCategories() => new(new SqliteContentDb(Open()));
    public DeleteSiteCategoryCommandHandler DeleteCategory() => new(new SqliteContentDb(Open()));
    public ImportOldSiteCategoriesCommandHandler ImportCategories() => new(new SqliteContentDb(Open()), Catalog, Time);
    public GetSiteCategoryQueryHandler Category() => new(new SqliteContentDb(Open()));
    public GetSiteCategoriesQueryHandler Categories() => new(new SqliteContentDb(Open()));

    /// <summary>The public catalog as one request sees it; <paramref name="cache"/> is shared to test its lifetime.</summary>
    public PublicCatalogSource Site(IMemoryCache? cache = null) =>
        new(new SqliteContentDb(Open()), Catalog, cache ?? new MemoryCache(new MemoryCacheOptions()));

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
