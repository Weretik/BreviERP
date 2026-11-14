using Crm.Domain.Embroidery.Settings;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryForm;

/// <summary>Hoops, extra costs and price names for the «Розрахунок» form; no rates for a Manager.</summary>
public sealed record GetEmbroideryFormQuery(EmbroideryActor Actor) : IQuery<Result<EmbroideryFormResponse>>;
