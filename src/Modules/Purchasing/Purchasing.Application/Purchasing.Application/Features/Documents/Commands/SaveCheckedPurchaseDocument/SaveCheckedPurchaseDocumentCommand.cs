using System.Text.Json;
using Purchasing.Application.Abstractions;
using Purchasing.Domain.Documents;
using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;

namespace Purchasing.Application.Features.Documents.Commands.SaveCheckedPurchaseDocument;

/// <summary>
/// «Завантажити JSON»: checked facts prepared by hand. <c>sourceUID</c> must be an active row of the file's message and
/// <c>originalFileLink</c> the file's exact link. The same facts again change nothing; different facts need
/// <paramref name="Replace"/>, and the old version goes to history.
/// </summary>
public sealed record SaveCheckedPurchaseDocumentCommand(string FileId, JsonElement Document, bool Replace, string By)
    : ICommand<Result<PurchaseDocumentSaveResponse>>;
