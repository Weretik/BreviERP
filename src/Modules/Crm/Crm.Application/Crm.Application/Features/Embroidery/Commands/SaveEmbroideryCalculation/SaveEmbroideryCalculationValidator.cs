using Crm.Domain.Embroidery.Saved;
using Crm.Application.Features.Embroidery.Queries.CalculateEmbroidery;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Commands.SaveEmbroideryCalculation;

public sealed class SaveEmbroideryCalculationValidator : AbstractValidator<SaveEmbroideryCalculationCommand>
{
    /// <summary>The typed name before trimming; longer names are cut to 120 after trimming.</summary>
    public const int RawNameMaxLength = 1000;

    public SaveEmbroideryCalculationValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Input).NotNull().ChildRules(input =>
        {
            input.RuleFor(x => x.Name).MaximumLength(RawNameMaxLength);
            input.RuleFor(x => x.CustomerPublicId).NotEqual(Guid.Empty).When(x => x.CustomerPublicId is not null);
            input.RuleFor(x => x.Calculation).NotNull().SetValidator(new CalculateEmbroideryInputValidator());
            input.RuleFor(x => x.Category).NotEmpty().WithMessage(EmbroideryMessages.CategoryNotFound)
                .MaximumLength(EmbroiderySavedCalculation.CategoryMaxLength);
        });
    }
}
