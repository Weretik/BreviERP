using System.Globalization;
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

    /// <summary>The first free slug of <see cref="ProductSlugCandidates"/>.</summary>
    private async Task<string> FreeSlugAsync(string name, string code, CancellationToken cancellationToken)
    {
        foreach (var candidate in ProductSlugCandidates.For(slugGenerator, name, code))
            if (!await repository.AnyAsync(new ProductByNameOrSlugSpec(name, candidate), cancellationToken))
                return candidate;
        return ProductSlugCandidates.Fallback();
    }

    private static Result<CreatedCatalogProduct> Invalid(string identifier, string message)
        => Result.Invalid(new ValidationError(identifier, message));
}
