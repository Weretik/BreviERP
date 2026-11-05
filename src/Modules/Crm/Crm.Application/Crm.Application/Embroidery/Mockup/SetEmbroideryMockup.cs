using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.Mapping;
using Crm.Application.Embroidery.Persistence;

namespace Crm.Application.Embroidery.Mockup;

/// <summary>«Макет»: picks a photo of the design from the Media library for the workshop, or clears it (EMB-001 R-14).</summary>
public sealed record SetEmbroideryMockupCommand(EmbroideryActor Actor, long Id, SetEmbroideryMockupInput Input)
    : ICommand<Result<EmbroiderySavedCalculationResponse>>;

public sealed class SetEmbroideryMockupCommandHandler(IEmbroideryStore store, IEmbroideryMediaReader media)
    : ICommandHandler<SetEmbroideryMockupCommand, Result<EmbroiderySavedCalculationResponse>>
{
    public async ValueTask<Result<EmbroiderySavedCalculationResponse>> Handle(SetEmbroideryMockupCommand command,
        CancellationToken cancellationToken)
    {
        var saved = await store.EmbroideryCalculations.SingleOrDefaultAsync(x => x.Id == command.Id,
            cancellationToken);
        if (saved is null) return Result.NotFound(EmbroideryMessages.CalculationNotFound);

        var mockups = await media.ReadForAsync([command.Input.MediaFileId], cancellationToken);
        if (command.Input.MediaFileId is { } fileId && !mockups.ContainsKey(fileId))
            return Result.Invalid(new ValidationError(EmbroideryMessages.MockupNotFound));

        saved.SetMockup(command.Input.MediaFileId);
        await store.SaveChangesAsync(cancellationToken);
        return Result.Success(saved.ToResponse(command.Actor.SeesCost, mockups.Of(saved.MockupMediaFileId)));
    }
}

public sealed class SetEmbroideryMockupCommandValidator : AbstractValidator<SetEmbroideryMockupCommand>
{
    public SetEmbroideryMockupCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.MediaFileId).GreaterThan(0).When(x => x.Input is { MediaFileId: not null });
    }
}
