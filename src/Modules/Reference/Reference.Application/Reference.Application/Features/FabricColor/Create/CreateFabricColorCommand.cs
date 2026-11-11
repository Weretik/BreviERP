using Reference.Application.Features.FabricColor.Shared;

namespace Reference.Application.Features.FabricColor.Create;

public sealed record CreateFabricColorCommand(FabricColorFields Request) : ICommand<Result<int>>;

public sealed class CreateFabricColorCommandValidator : AbstractValidator<CreateFabricColorCommand>
{
    public CreateFabricColorCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Запит на створення кольору не може бути порожнім.");
        When(x => x.Request is not null, () => RuleFor(x => x.Request).SetValidator(new FabricColorFieldsValidator()));
    }
}
