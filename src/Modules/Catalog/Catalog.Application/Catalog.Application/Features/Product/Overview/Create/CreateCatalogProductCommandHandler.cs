using System.Globalization;
using System.Text;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Contracts.SlugGeneration;
using Catalog.Application.Features.Product.Overview.Create.Specifications;
using Catalog.Application.Features.Product.Overview.Shared;
using Catalog.Application.Features.Product.Overview.Shared.Specifications;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Overview.Create;

public sealed class CreateCatalogProductCommandHandler(
    ICatalogRepository<ProductEntity> repository,
    IProductSectionReader sectionReader,
    IProductReferenceReader references,
    IPpePricingInputReader pricingInputs,
    IPpeOfferVersionWriter offers,
    IProductSlugGenerator slugGenerator,
    TimeProvider timeProvider) : ICommandHandler<CreateCatalogProductCommand, Result<CreatedCatalogProduct>>
{
    public const string OfferSource = "Створення ЗІЗ";
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public async ValueTask<Result<CreatedCatalogProduct>> Handle(
        CreateCatalogProductCommand command, CancellationToken cancellationToken)
    {
        if (!ProductOverviewTypes.TryParse(command.Type, out var type))
            return Invalid(nameof(command.Type), "Тип товару: Sewing або Ppe.");
        var name = command.Name.Trim();

        var section = await sectionReader.FindAsync(command.SectionId, cancellationToken);
        if (section is null) return Invalid(nameof(command.SectionId), "Розділ не знайдено.");
        if (section.Type != ProductOverviewTypes.Name(type))
            return Invalid(nameof(command.SectionId), "Розділ належить іншому типу товарів.");

        var code = string.IsNullOrWhiteSpace(command.Code) ? null : command.Code.Trim();
        if (code is null && type == ProductType.Sewing)
            code = SewingModelCodes.Next(await repository.ListAsync(new SewingModelCodesSpec(), cancellationToken));
        if (code is null) return Invalid(nameof(command.Code), "Для ЗІЗ потрібен артикул сайту.");
        if (await repository.AnyAsync(new ProductByCodeSpec(type, code), cancellationToken))
            return Result.Conflict($"Код «{code}» уже має інший товар.");
        if (await repository.AnyAsync(new ProductByNameOrSlugSpec(name), cancellationToken))
            return Result.Conflict($"Товар «{name}» уже є. Додайте до назви колір, розмір або інше уточнення.");

        if (command.Ppe is { } ppe)
        {
            var snapshot = await references.GetSnapshotAsync(cancellationToken);
            if (!snapshot.Suppliers.ContainsKey(ppe.SupplierId))
                return Invalid("Ppe.SupplierId", "Постачальника не знайдено.");
            if (await pricingInputs.ReadRuleAsync(ppe.RuleCode.Trim(), cancellationToken) is null)
                return Invalid("Ppe.RuleCode", "Правило ціноутворення не знайдено.");
        }

        var slug = await FreeSlugAsync(name, code, cancellationToken);
        var lastId = await repository.FirstOrDefaultAsync(new LastProductIdSpec(), cancellationToken);
        var id = ProductId.Create(lastId.Value + 1);
        var now = timeProvider.GetUtcNow();
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, Kyiv).DateTime);

        // Russian names are not kept for new products; the column stays filled until the Content module owns texts.
        var product = ProductEntity.Create(id, name, name, ProductSlug.Create(slug), type, now);
        product.SetCode(code, now);
        product.SetOverview(new ProductOverviewDetails(command.SectionId, command.Season, command.IsActive,
            command.IsVerified, null, command.CurrentSiteUrl, command.QuoteDescription), today, now);
        if (command.Ppe is { } supplier)
            product.SetPpeSupplier(new PpeSupplierDetails(SupplierReferenceId.Create(supplier.SupplierId),
                supplier.SupplierSku, supplier.SupplierUrl, supplier.SupplierPrice, supplier.AllowManualPrice,
                supplier.RuleCode.Trim(), supplier.Unit), now);
        await repository.AddAsync(product, cancellationToken);

        var priceSaved = true;
        if (command.Ppe is { } price)
        {
            var values = new PpeOfferValues(price.RuleCode.Trim(), price.SupplierPrice, null, null, OfferSource,
                today.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), true);
            var appended = await offers.AppendAsync(id.Value, null, Guid.NewGuid(), values, command.Actor, cancellationToken);
            priceSaved = appended.IsSuccess;
        }

        return Result.Success(new CreatedCatalogProduct(id.Value, ProductOverviewTypes.Name(type), code, priceSaved));
    }

    /// <summary>The slug from the name; when taken, the code is added, then a number.</summary>
    private async Task<string> FreeSlugAsync(string name, string code, CancellationToken cancellationToken)
    {
        var baseSlug = Trim(slugGenerator.GenerateFromUkrainianName(name));
        var codeSlug = CodeSlug(code);
        var candidates = new List<string> { baseSlug };
        if (codeSlug.Length > 0) candidates.Add(Trim(JoinSlug(baseSlug, codeSlug)));
        for (var n = 2; n <= 50; n++) candidates.Add(Trim(JoinSlug(baseSlug, codeSlug)) + "-" + n);

        foreach (var candidate in candidates.Where(x => x.Length > 0).Distinct())
            if (!await repository.AnyAsync(new ProductByNameOrSlugSpec(name, candidate), cancellationToken))
                return candidate;
        return "product-" + Guid.NewGuid().ToString("N")[..12];
    }

    private static string JoinSlug(string left, string right)
        => left.Length == 0 ? right : right.Length == 0 ? left : left + "-" + right;

    private static string CodeSlug(string code)
    {
        var builder = new StringBuilder();
        foreach (var character in code.ToLowerInvariant())
            builder.Append(character is >= 'a' and <= 'z' or >= '0' and <= '9' ? character : '-');
        return string.Join('-', builder.ToString().Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>Keeps room for a suffix under the 160-character slug limit.</summary>
    private static string Trim(string slug) => slug.Length <= 140 ? slug : slug[..140].TrimEnd('-');

    private static Result<CreatedCatalogProduct> Invalid(string identifier, string message)
        => Result.Invalid(new ValidationError(identifier, message));
}
