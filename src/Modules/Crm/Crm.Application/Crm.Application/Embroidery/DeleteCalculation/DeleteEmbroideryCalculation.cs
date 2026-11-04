using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Persistence;

namespace Crm.Application.Embroidery.DeleteCalculation;

/// <summary>Removes a saved calculation; Director and Admin only.</summary>
public sealed record DeleteEmbroideryCalculationCommand(EmbroideryActor Actor, long Id) : ICommand<Result>;

public sealed class DeleteEmbroideryCalculationCommandHandler(IEmbroideryStore store)
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

public sealed class DeleteEmbroideryCalculationCommandValidator : AbstractValidator<DeleteEmbroideryCalculationCommand>
{
    public DeleteEmbroideryCalculationCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
