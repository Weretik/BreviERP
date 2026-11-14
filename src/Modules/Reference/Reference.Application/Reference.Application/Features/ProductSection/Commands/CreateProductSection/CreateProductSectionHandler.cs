using Reference.Application.Features.ProductSection.Shared;
using Reference.Domain.ProductSections;
using ProductSectionEntity = Reference.Domain.ProductSections.ProductSection;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.ProductSection.Commands.CreateProductSection;

public sealed class CreateProductSectionHandler(IReferenceRepository<ProductSectionEntity> repository)
    : ICommandHandler<CreateProductSectionCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(CreateProductSectionCommand command, CancellationToken cancellationToken)
    {
        var name = command.Request.Name.Trim();
        if (await repository.AnyAsync(new ProductSectionByNameSpec(name), cancellationToken))
            return Result.Invalid([new ValidationError("Request.Name", "Розділ з такою назвою вже існує.")]);

        var existing = await repository.ListAsync(cancellationToken);
        var nextId = existing.Count == 0 ? 1 : existing.Max(x => x.Id.Value) + 1;

        var created = ProductSectionEntity.Create(ProductSectionId.From(nextId),
            ProductSectionDetailsFactory.From(command.Request));
        if (created.IsFailure) return created.Error!.ToResult<int>();
        var entity = created.Value;

        await repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id.Value);
    }
}
