using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Commands.SetEmbroideryMockup;

/// <summary>«Макет»: picks a photo of the design from the Media library for the workshop, or clears it (EMB-001 R-14).</summary>
public sealed record SetEmbroideryMockupCommand(EmbroideryActor Actor, long Id, SetEmbroideryMockupInput Input)
    : ICommand<Result<EmbroiderySavedCalculationResponse>>;
