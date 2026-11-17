namespace Purchasing.Application.Features.Documents.Commands.StartNewPurchaseFilesRecognition;

public sealed class StartNewPurchaseFilesRecognitionValidator : AbstractValidator<StartNewPurchaseFilesRecognitionCommand>
{
    public StartNewPurchaseFilesRecognitionValidator() =>
        RuleFor(x => x.By).NotEmpty().MaximumLength(Domain.Documents.PurchaseCheckedDocument.ByMaxLength);
}
