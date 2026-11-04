using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Domain.FabricNorms;

namespace Production.Application.FabricNorms.SaveNorm;

public sealed class SaveFabricNormCommandValidator : AbstractValidator<SaveFabricNormCommand>
{
    public SaveFabricNormCommandValidator()
    {
        RuleFor(x => x.ModelCode).NotEmpty().MaximumLength(ModelFabricNorm.ModelCodeMaxLength);
        RuleFor(x => x.Norm).NotNull().SetValidator(new FabricNormInputValidator());
    }
}
