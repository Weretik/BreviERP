using Production.Application.Abstractions;
using Production.Domain.FabricNorms;

namespace Production.Application.Features.FabricNorms.Commands.DeleteFabricNorm;

public sealed class DeleteFabricNormValidator : AbstractValidator<DeleteFabricNormCommand>
{
    public DeleteFabricNormValidator() =>
        RuleFor(x => x.ModelCode).NotEmpty().MaximumLength(ModelFabricNorm.ModelCodeMaxLength);
}
