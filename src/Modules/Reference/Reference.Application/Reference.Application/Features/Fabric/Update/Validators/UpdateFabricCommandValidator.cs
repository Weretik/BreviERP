using Reference.Application.Features.Fabric.Shared.Validators;

namespace Reference.Application.Features.Fabric.Update.Validators;

public sealed class UpdateFabricCommandValidator : AbstractValidator<UpdateFabricCommand>
{
    public UpdateFabricCommandValidator()
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
