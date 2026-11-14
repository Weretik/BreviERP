using Purchasing.Application.Features.Documents.Shared;

namespace Purchasing.Application.Features.Documents.Commands.RecognizePurchaseFile;

public sealed class RecognizePurchaseFileValidator : AbstractValidator<RecognizePurchaseFileCommand>
{
    public RecognizePurchaseFileValidator()
    {
        RuleFor(x => x.FileId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.By).NotEmpty().MaximumLength(Domain.Documents.PurchaseCheckedDocument.ByMaxLength);
    }
}
