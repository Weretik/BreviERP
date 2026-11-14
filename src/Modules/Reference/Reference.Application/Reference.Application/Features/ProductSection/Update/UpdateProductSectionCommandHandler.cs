using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.ProductSection.Shared;
using Reference.Application.Features.ProductSection.Shared.Specifications;
using ProductSectionEntity = Reference.Domain.ProductSections.Entities.ProductSection;

namespace Reference.Application.Features.ProductSection.Update;

/// <summary>Edits a section. Products keep their link: they refer to the section by ID, not by name.</summary>
public sealed class UpdateProductSectionCommandHandler(IReferenceRepository<ProductSectionEntity> repository)
    : ICommandHandler<UpdateProductSectionCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateProductSectionCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(new ProductSectionByIdSpec(command.Id), cancellationToken);
        if (entity is null)
            return Result.NotFound();

        var name = command.Request.Name.Trim();
        if (await repository.AnyAsync(new ProductSectionByNameSpec(name, command.Id), cancellationToken))
            return Result.Invalid([new ValidationError("Request.Name", "Розділ з такою назвою вже існує.")]);

        var changed = entity.ApplyDetails(ProductSectionDetailsFactory.From(command.Request));
        if (changed.IsFailure) return changed.Error!.ToResult();

        await repository.UpdateAsync(entity, cancellationToken);
        return Result.Success();
    }
}
