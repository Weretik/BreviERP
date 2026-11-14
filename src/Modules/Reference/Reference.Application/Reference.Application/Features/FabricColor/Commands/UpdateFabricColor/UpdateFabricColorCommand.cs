using Reference.Application.Features.FabricColor.Shared;

namespace Reference.Application.Features.FabricColor.Commands.UpdateFabricColor;

public sealed record UpdateFabricColorCommand(int Id, FabricColorFields Request) : ICommand<Result>;

public sealed class UpdateFabricColorValidator : AbstractValidator<UpdateFabricColorCommand>
{
    public UpdateFabricColorValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Ідентифікатор кольору має бути більшим за 0.");
        RuleFor(x => x.Request).NotNull().WithMessage("Запит на зміну кольору не може бути порожнім.");
        When(x => x.Request is not null, () => RuleFor(x => x.Request).SetValidator(new FabricColorFieldsValidator()));
    }
}
