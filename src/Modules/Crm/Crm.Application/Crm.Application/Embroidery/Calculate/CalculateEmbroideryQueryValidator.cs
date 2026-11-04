using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.Validation;
using Crm.Domain.Embroidery.Calculation;

namespace Crm.Application.Embroidery.Calculate;

public sealed class CalculateEmbroideryQueryValidator : AbstractValidator<CalculateEmbroideryQuery>
{
    /// <summary>Business ceilings the old calculator did not have; they only stop typing mistakes.</summary>
    public const int MaxQuantity = 1_000_000;
    public const int MaxStitches = 1_000_000;
    public const int MaxLayers = 20;
    public const int MaxPerHoop = 500;
    public const int MaxMinutes = 100_000;

    public CalculateEmbroideryQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Input).NotNull().SetValidator(new CalculateEmbroideryInputValidator());
    }
}

/// <summary>The «Розрахунок» form rules; also used when a calculation is saved.</summary>
public sealed class CalculateEmbroideryInputValidator : AbstractValidator<CalculateEmbroideryInput>
{
    private const int MaxQuantity = CalculateEmbroideryQueryValidator.MaxQuantity;
    private const int MaxStitches = CalculateEmbroideryQueryValidator.MaxStitches;
    private const int MaxLayers = CalculateEmbroideryQueryValidator.MaxLayers;
    private const int MaxPerHoop = CalculateEmbroideryQueryValidator.MaxPerHoop;
    private const int MaxMinutes = CalculateEmbroideryQueryValidator.MaxMinutes;

    public CalculateEmbroideryInputValidator()
    {
        RuleFor(x => x.Mode).Must(EmbroideryCodes.Is<EmbroideryMode>).WithMessage("Оберіть, що вишиваємо.");
        RuleFor(x => x.Program).Must(EmbroideryCodes.Is<EmbroideryProgram>).WithMessage("Оберіть програму.");
        RuleFor(x => x.MaterialMode).Must(EmbroideryCodes.Is<EmbroideryMaterialMode>)
            .WithMessage("Оберіть, як рахувати матеріали.");
        RuleFor(x => x.Quantity).InclusiveBetween(1, MaxQuantity).WithMessage(EmbroideryCalculator.QuantityRequired);
        RuleFor(x => x.StitchesPerUnit).InclusiveBetween(1, MaxStitches)
            .WithMessage(EmbroideryCalculator.StitchesRequired);
        RuleFor(x => x.HoopId).NotEmpty().WithMessage(EmbroideryCalculator.HoopNotFound).MaximumLength(64);
        RuleFor(x => x.StabilizerLayers).InclusiveBetween(1, MaxLayers).WithMessage(EmbroideryCalculator.LayersRequired);
        RuleFor(x => x.FabricLayers).InclusiveBetween(1, MaxLayers).WithMessage(EmbroideryCalculator.LayersRequired)
            .When(x => x.Mode == nameof(EmbroideryMode.Chevron));
        RuleFor(x => x.UnitsPerHoop).InclusiveBetween(1, MaxPerHoop).WithMessage(EmbroideryCalculator.PerHoopRequired)
            .When(x => x.Mode == nameof(EmbroideryMode.Chevron));
        RuleFor(x => x.ProgramMinutes).InclusiveBetween(0, MaxMinutes)
            .WithMessage("Час розробки: від 0 хвилин.");
        RuleFor(x => x.MachineMinutes!.Value).InclusiveBetween(0, MaxMinutes)
            .WithMessage("Час роботи машини: від 0 хвилин.").When(x => x.MachineMinutes is not null);
        RuleFor(x => x.ExpenseOverrides).Must(x => x is null || x.Count <= 100);
    }
}
