using Accounting.Application.Features.Counterparties.Shared;

namespace Accounting.Api.Endpoints.Counterparties.MergeFinanceCounterparty;

/// <summary>«Об'єднати»: the target and both versions the user saw.</summary>
public sealed record FinanceCounterpartyMergeInput(Guid TargetId, int Version, int TargetVersion);
