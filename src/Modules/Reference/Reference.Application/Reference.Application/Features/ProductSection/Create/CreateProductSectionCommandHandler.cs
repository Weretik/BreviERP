using BuildingBlocks.Domain.Exceptions;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.ProductSection.Shared;
using Reference.Application.Features.ProductSection.Shared.Specifications;
using Reference.Domain.ProductSections.ValueObjects;
using ProductSectionEntity = Reference.Domain.ProductSections.Entities.ProductSection;

namespace Reference.Application.Features.ProductSection.Create;

public sealed class CreateProductSectionCommandHandler(IReferenceRepository<ProductSectionEntity> repository)
    : ICommandHandler<CreateProductSectionCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(CreateProductSectionCommand command, CancellationToken cancellationToken)
    {
        var name = command.Request.Name.Trim();
        if (await repository.AnyAsync(new ProductSectionByNameSpec(name), cancellationToken))
            return Result.Invalid([new ValidationError("Request.Name", "Розділ з такою назвою вже існує.")]);

        var existing = await repository.ListAsync(cancellationToken);
        var nextId = existing.Count == 0 ? 1 : existing.Max(x => x.Id.Value) + 1;

        ProductSectionEntity entity;
        try
        {
            entity = ProductSectionEntity.Create(ProductSectionId.From(nextId),
                ProductSectionDetailsFactory.From(command.Request));
        }
        catch (DomainException exception)
        {
            return Result.Invalid([new ValidationError("Request", exception.Message)]);
        }

        await repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id.Value);
    }
}
