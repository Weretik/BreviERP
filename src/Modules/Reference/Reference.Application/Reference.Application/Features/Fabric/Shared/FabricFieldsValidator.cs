using FabricEntity = Reference.Domain.GarmentAccessories.Fabric;

namespace Reference.Application.Features.Fabric.Shared;

public sealed class FabricFieldsValidator : AbstractValidator<IFabricFields>
{
    public FabricFieldsValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Назва тканини є обов'язковою.")
            .MaximumLength(FabricEntity.NameMaxLength).WithMessage("Назва тканини не може перевищувати 500 символів.");

        RuleFor(x => x.Price)
            .InclusiveBetween(0, 10_000).WithMessage("Ціна тканини має бути від 0 до 10000.");

        RuleFor(x => x.ProviderName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Назва постачальника тканини є обов'язковою.")
            .MaximumLength(200).WithMessage("Назва постачальника тканини не може перевищувати 200 символів.");

        RuleFor(x => x.Code)
            .MaximumLength(FabricEntity.CodeMaxLength).WithMessage("Код тканини не може перевищувати 20 символів.");
        RuleFor(x => x.NameRu)
            .MaximumLength(FabricEntity.NameMaxLength).WithMessage("Назва (RU) не може перевищувати 500 символів.");
        RuleFor(x => x.Composition)
            .MaximumLength(FabricEntity.CompositionMaxLength).WithMessage("Склад не може перевищувати 500 символів.");
        RuleFor(x => x.CompositionRu)
            .MaximumLength(FabricEntity.CompositionMaxLength).WithMessage("Склад (RU) не може перевищувати 500 символів.");
        RuleFor(x => x.Density)
            .MaximumLength(FabricEntity.DensityMaxLength).WithMessage("Щільність не може перевищувати 100 символів.");
        RuleFor(x => x.Comment)
            .MaximumLength(FabricEntity.CommentMaxLength).WithMessage("Коментар не може перевищувати 1000 символів.");
        RuleFor(x => x.ProposalDescription)
            .MaximumLength(FabricEntity.ProposalDescriptionMaxLength).WithMessage("Опис для КП не може перевищувати 1000 символів.");
    }
}
