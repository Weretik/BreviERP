using Reference.Application.Features.FabricColor.Shared;

namespace Reference.Application.Features.FabricColor.Commands.CreateFabricColor;

public sealed record CreateFabricColorCommand(FabricColorFields Request) : ICommand<Result<int>>;

public sealed class CreateFabricColorValidator : AbstractValidator<CreateFabricColorCommand>
{
    public CreateFabricColorValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Запит на створення кольору не може бути порожнім.");
        When(x => x.Request is not null, () => RuleFor(x => x.Request).SetValidator(new FabricColorFieldsValidator()));
    }
}
