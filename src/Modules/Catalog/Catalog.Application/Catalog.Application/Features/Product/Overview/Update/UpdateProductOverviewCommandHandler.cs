using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Product.Overview.Shared;
using Catalog.Application.Features.Product.Overview.Update.Specifications;
using Catalog.Domain.Products.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Overview.Update;

public sealed class UpdateProductOverviewCommandHandler(
    ICatalogRepository<ProductEntity> repository,
    IProductSectionReader sectionReader,
    TimeProvider timeProvider) : ICommandHandler<UpdateProductOverviewCommand, Result>
{

    public async ValueTask<Result> Handle(UpdateProductOverviewCommand command, CancellationToken cancellationToken)
    {
        var product = await repository.FirstOrDefaultAsync(new ProductOverviewByIdSpec(command.Id), cancellationToken);
        if (product is null) return Result.NotFound();

        var code = string.IsNullOrWhiteSpace(command.Code) ? null : command.Code.Trim();
        if (code is null && product.Type == Domain.Products.Enums.ProductType.Ppe)
            return Result.Invalid(new ValidationError(nameof(command.Code), "Для ЗІЗ потрібен артикул сайту."));
        if (code is not null &&
            await repository.AnyAsync(new ProductByCodeExceptIdSpec(product.Type, code, command.Id), cancellationToken))
            return Result.Conflict($"Код «{code}» уже має інший товар.");

        if (command.SectionId is { } sectionId)
        {
            var section = await sectionReader.FindAsync(sectionId, cancellationToken);
            if (section is null)
                return Result.Invalid(new ValidationError(nameof(command.SectionId), "Розділ не знайдено."));
            if (section.Type != ProductOverviewTypes.Name(product.Type))
                return Result.Invalid(new ValidationError(nameof(command.SectionId), "Розділ належить іншому типу товарів."));
        }

        var now = timeProvider.GetUtcNow();
        var changed = product.SetCode(code, now);
        if (changed.IsSuccess)
            changed = product.SetOverview(new ProductOverviewDetails(command.SectionId, command.Season,
                command.IsActive, command.CurrentSiteUrl, command.QuoteDescription), now);
        if (changed.IsFailure) return changed.Error!.ToResult();
        await repository.UpdateAsync(product, cancellationToken);
        return Result.Success();
    }
}
