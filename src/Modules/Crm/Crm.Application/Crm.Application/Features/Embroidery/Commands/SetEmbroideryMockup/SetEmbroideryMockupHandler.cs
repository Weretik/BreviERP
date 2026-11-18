using BuildingBlocks.Domain.Identifiers.Products;
using Crm.Domain.Embroidery.Saved;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;
using Crm.Application.Abstractions.Embroidery;

namespace Crm.Application.Features.Embroidery.Commands.SetEmbroideryMockup;

public sealed class SetEmbroideryMockupHandler(IEmbroideryStore store, IEmbroideryMediaReader media)
    : ICommandHandler<SetEmbroideryMockupCommand, Result<EmbroiderySavedCalculationResponse>>
{
    public async ValueTask<Result<EmbroiderySavedCalculationResponse>> Handle(SetEmbroideryMockupCommand command,
        CancellationToken cancellationToken)
    {
        var saved = await store.EmbroideryCalculations.SingleOrDefaultAsync(x => x.Id == new EmbroiderySavedCalculationId(command.Id),
            cancellationToken);
        if (saved is null) return Result.NotFound(EmbroideryMessages.CalculationNotFound);

        var mockups = await media.ReadForAsync([MediaFileId.FromNullable(command.Input.MediaFileId)], cancellationToken);
        if (command.Input.MediaFileId is { } fileId && !mockups.ContainsKey(fileId))
            return Result.Invalid(new ValidationError(EmbroideryMessages.MockupNotFound));

        saved.SetMockup(MediaFileId.FromNullable(command.Input.MediaFileId));
        await store.SaveChangesAsync(cancellationToken);
        return Result.Success(saved.ToResponse(command.Actor.SeesCost, mockups.Of(saved.MockupMediaFileId)));
    }
}
