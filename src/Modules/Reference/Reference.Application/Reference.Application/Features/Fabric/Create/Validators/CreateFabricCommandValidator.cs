using Reference.Application.Features.Fabric.Shared.Validators;

namespace Reference.Application.Features.Fabric.Create.Validators;

public sealed class CreateFabricCommandValidator : AbstractValidator<CreateFabricCommand>
{
    public CreateFabricCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на створення тканини не може бути порожнім.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Id)
                .GreaterThan(0)
                .WithMessage("Ідентифікатор тканини має бути більшим за 0.");

            RuleFor(x => x.Request).SetValidator(new FabricFieldsValidator());
        });
    }
}
