using Crm.Application.Features.Embroidery.Shared.Access;

namespace Crm.Application.Features.Embroidery.Commands.DeleteEmbroideryCalculation;

/// <summary>Removes a saved calculation; Director and Admin only.</summary>
public sealed record DeleteEmbroideryCalculationCommand(EmbroideryActor Actor, long Id) : ICommand<Result>;
