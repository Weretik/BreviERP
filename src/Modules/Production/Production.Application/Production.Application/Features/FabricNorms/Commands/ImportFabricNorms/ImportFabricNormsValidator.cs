using Production.Application.Abstractions;
using Production.Domain.FabricNorms;
using Production.Application.Features.FabricNorms.Shared;

namespace Production.Application.Features.FabricNorms.Commands.ImportFabricNorms;

public sealed class ImportFabricNormsValidator : AbstractValidator<ImportFabricNormsCommand>
{
    public const int MaxRows = 1000;

    public ImportFabricNormsValidator()
    {
        RuleFor(x => x.Rows).NotEmpty().Must(x => x is null || x.Count <= MaxRows)
            .WithMessage($"Не більше {MaxRows} норм за раз.");
        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(x => x.ModelCode).NotEmpty().MaximumLength(ModelFabricNorm.ModelCodeMaxLength);
            row.RuleFor(x => x.Norm).NotNull().SetValidator(new FabricNormInputValidator());
        });
        RuleFor(x => x.Rows).Must(x => x is null || x.Where(r => !string.IsNullOrWhiteSpace(r.ModelCode))
                .GroupBy(r => r.ModelCode.Trim().ToUpperInvariant()).All(g => g.Count() == 1))
            .WithMessage("У файлі є однакові коди моделей.");
    }
}
