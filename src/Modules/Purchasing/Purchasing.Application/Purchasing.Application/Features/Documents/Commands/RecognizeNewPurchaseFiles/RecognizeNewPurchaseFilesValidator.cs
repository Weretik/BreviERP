using Purchasing.Application.Abstractions;
using Purchasing.Domain.Ingestion;
using Purchasing.Application.Features.Documents.Shared;

namespace Purchasing.Application.Features.Documents.Commands.RecognizeNewPurchaseFiles;

public sealed class RecognizeNewPurchaseFilesValidator : AbstractValidator<RecognizeNewPurchaseFilesCommand>
{
    public RecognizeNewPurchaseFilesValidator()
    {
        RuleFor(x => x.By).NotEmpty().MaximumLength(Domain.Documents.PurchaseCheckedDocument.ByMaxLength);
        RuleFor(x => x.BatchSize).InclusiveBetween(1, 20);
    }
}
