using Purchasing.Application.Features.Documents.Shared;

namespace Purchasing.Application.Features.Documents.Commands.RecognizePurchaseFile;

/// <summary>
/// «Розпізнати» / «Розпізнати знову» of one file. A new checked document replaces the old one, which goes to history.
/// Unavailable when the AI is off, has no key or reached its limit; Conflict while the sheet sync runs.
/// </summary>
public sealed record RecognizePurchaseFileCommand(string FileId, string By) : ICommand<Result<PurchaseRecognitionResponse>>;

public sealed class RecognizePurchaseFileValidator : AbstractValidator<RecognizePurchaseFileCommand>
{
    public RecognizePurchaseFileValidator()
    {
        RuleFor(x => x.FileId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.By).NotEmpty().MaximumLength(Domain.Documents.PurchaseCheckedDocument.ByMaxLength);
    }
}

public sealed class RecognizePurchaseFileHandler(PurchaseFileRecognition recognition)
    : ICommandHandler<RecognizePurchaseFileCommand, Result<PurchaseRecognitionResponse>>
{
    public async ValueTask<Result<PurchaseRecognitionResponse>> Handle(RecognizePurchaseFileCommand command,
        CancellationToken cancellationToken) =>
        await recognition.RecognizeAsync(command.FileId, command.By, cancellationToken);
}
