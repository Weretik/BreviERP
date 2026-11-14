using Crm.Domain.Embroidery.Settings;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Commands.SaveEmbroiderySettings;

public sealed record SaveEmbroiderySettingsCommand(EmbroideryActor Actor, SaveEmbroiderySettingsInput Input)
    : ICommand<Result<EmbroiderySettingsResponse>>;
