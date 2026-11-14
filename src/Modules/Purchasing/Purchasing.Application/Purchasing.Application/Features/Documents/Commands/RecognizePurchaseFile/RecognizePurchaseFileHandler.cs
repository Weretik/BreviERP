using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Application.Features.Documents.Shared.Recognition;

namespace Purchasing.Application.Features.Documents.Commands.RecognizePurchaseFile;

public sealed class RecognizePurchaseFileHandler(PurchaseFileRecognition recognition)
    : ICommandHandler<RecognizePurchaseFileCommand, Result<PurchaseRecognitionResponse>>
{
    public async ValueTask<Result<PurchaseRecognitionResponse>> Handle(RecognizePurchaseFileCommand command,
        CancellationToken cancellationToken) =>
        await recognition.RecognizeAsync(command.FileId, command.By, cancellationToken);
}
