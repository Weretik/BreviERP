using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Transfers;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transfers.Shared;

namespace Accounting.Application.Features.Transfers.Commands.VoidFinanceTransfer;

/// <summary>«Анулювати»: the transfer stays in the journal and «Історія» but no longer moves money.</summary>
public sealed record VoidFinanceTransferCommand(Guid Id, int Version, string By) : ICommand<Result<FinanceTransferResponse>>;
