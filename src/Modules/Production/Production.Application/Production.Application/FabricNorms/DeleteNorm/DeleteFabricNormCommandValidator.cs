using Production.Application.Abstractions;
using Production.Domain.FabricNorms;

namespace Production.Application.FabricNorms.DeleteNorm;

public sealed class DeleteFabricNormCommandValidator : AbstractValidator<DeleteFabricNormCommand>
{
    public DeleteFabricNormCommandValidator() =>
        RuleFor(x => x.ModelCode).NotEmpty().MaximumLength(ModelFabricNorm.ModelCodeMaxLength);
}
