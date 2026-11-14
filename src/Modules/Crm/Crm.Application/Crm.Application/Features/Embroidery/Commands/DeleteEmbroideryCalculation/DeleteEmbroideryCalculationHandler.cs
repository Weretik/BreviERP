using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Abstractions.Embroidery;

namespace Crm.Application.Features.Embroidery.Commands.DeleteEmbroideryCalculation;

public sealed class DeleteEmbroideryCalculationHandler(IEmbroideryStore store)
    : ICommandHandler<DeleteEmbroideryCalculationCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteEmbroideryCalculationCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.SeesCost) return Result.Forbidden(EmbroideryMessages.DeleteForbidden);

        var saved = await store.EmbroideryCalculations.SingleOrDefaultAsync(x => x.Id == command.Id,
            cancellationToken);
        if (saved is null) return Result.NotFound(EmbroideryMessages.CalculationNotFound);

        store.EmbroideryCalculations.Remove(saved);
        await store.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
