using Crm.Domain.Embroidery.Saved;
using Crm.Application.Features.Embroidery.Queries.CalculateEmbroidery;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Commands.SaveEmbroideryCalculation;

/// <summary>
/// «Зберегти розрахунок»: the server prices the form with the rates in force now (the browser sends no prices),
/// freezes those rates and stores a new record with the one price category chosen. An «Орієнтовна» price can be saved; a price that failed cannot.
/// </summary>
public sealed record SaveEmbroideryCalculationCommand(EmbroideryActor Actor, SaveEmbroideryCalculationInput Input)
    : ICommand<Result<EmbroiderySavedCalculationResponse>>;
