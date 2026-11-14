using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Transfers;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transfers.Shared;

namespace Accounting.Application.Features.Transfers.Commands.UpdateFinanceTransfer;

/// <summary>«Змінити» an active hand-made transfer; the area stays, a stale <paramref name="Version"/> gets 409.</summary>
public sealed record UpdateFinanceTransferCommand(Guid Id, FinanceTransferFields Fields, int Version, string By)
    : ICommand<Result<FinanceTransferResponse>>;
