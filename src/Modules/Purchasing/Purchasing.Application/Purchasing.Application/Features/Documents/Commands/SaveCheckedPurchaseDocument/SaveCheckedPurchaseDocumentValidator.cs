using System.Text.Json;
using Purchasing.Application.Abstractions;
using Purchasing.Domain.Documents;
using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;

namespace Purchasing.Application.Features.Documents.Commands.SaveCheckedPurchaseDocument;

public sealed class SaveCheckedPurchaseDocumentValidator : AbstractValidator<SaveCheckedPurchaseDocumentCommand>
{
    public SaveCheckedPurchaseDocumentValidator()
    {
        RuleFor(x => x.FileId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.By).NotEmpty().MaximumLength(PurchaseCheckedDocument.ByMaxLength);
    }
}
