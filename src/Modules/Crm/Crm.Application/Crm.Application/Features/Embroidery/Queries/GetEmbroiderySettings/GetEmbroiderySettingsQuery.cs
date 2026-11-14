using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroiderySettings;

/// <summary>«Налаштування» with the embroiderer's rate from «Параметри ціноутворення»; Director and Admin only.</summary>
public sealed record GetEmbroiderySettingsQuery(EmbroideryActor Actor) : IQuery<Result<EmbroiderySettingsResponse>>;
