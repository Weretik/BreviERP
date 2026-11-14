using Production.Application.Abstractions;
using Production.Domain.FabricNorms;
using Production.Application.Features.FabricNorms.Shared;

namespace Production.Application.Features.FabricNorms.Commands.SaveFabricNorm;

public sealed class SaveFabricNormValidator : AbstractValidator<SaveFabricNormCommand>
{
    public SaveFabricNormValidator()
    {
        RuleFor(x => x.ModelCode).NotEmpty().MaximumLength(ModelFabricNorm.ModelCodeMaxLength);
        RuleFor(x => x.Norm).NotNull().SetValidator(new FabricNormInputValidator());
    }
}
