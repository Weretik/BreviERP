using Reference.Application.Features.Fabric.Shared;

namespace Reference.Application.Features.Fabric.Commands.UpdateFabric;

public sealed class UpdateFabricValidator : AbstractValidator<UpdateFabricCommand>
{
    public UpdateFabricValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор тканини має бути більшим за 0.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на оновлення тканини не може бути порожнім.");

        When(x => x.Request is not null, () => RuleFor(x => x.Request).SetValidator(new FabricFieldsValidator()));
    }
}
