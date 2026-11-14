using Crm.Application.Features.Embroidery.Shared.Access;

namespace Crm.Application.Features.Embroidery.Commands.DeleteEmbroideryCalculation;

public sealed class DeleteEmbroideryCalculationValidator : AbstractValidator<DeleteEmbroideryCalculationCommand>
{
    public DeleteEmbroideryCalculationValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
