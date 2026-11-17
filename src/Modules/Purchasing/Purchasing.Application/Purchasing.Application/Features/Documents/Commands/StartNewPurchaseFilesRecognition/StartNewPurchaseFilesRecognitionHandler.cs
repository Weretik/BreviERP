using Purchasing.Application.Abstractions;
using Purchasing.Application.Features.Documents.Shared;

namespace Purchasing.Application.Features.Documents.Commands.StartNewPurchaseFilesRecognition;

public sealed class StartNewPurchaseFilesRecognitionHandler(IPurchasingDbContext db, IPurchaseRecognitionLauncher launcher)
    : ICommandHandler<StartNewPurchaseFilesRecognitionCommand, Result<PurchaseRecognitionStartResponse>>
{
    public const string AlreadyStarted =
        "Розпізнавання вже запущено. Результат буде на сторінці «Фонові задачі», нові документи з'являться в списку.";

    public async ValueTask<Result<PurchaseRecognitionStartResponse>> Handle(StartNewPurchaseFilesRecognitionCommand command,
        CancellationToken cancellationToken)
    {
        var waiting = (await NewPurchaseFiles.IdsAsync(db, cancellationToken)).Count;
        if (waiting == 0) return Result.Success(new PurchaseRecognitionStartResponse(0, false));

        var requested = await launcher.RequestAsync(command.By, cancellationToken);
        if (requested.Status == ResultStatus.Conflict)
            return Result<PurchaseRecognitionStartResponse>.Conflict(AlreadyStarted);
        return requested.IsSuccess
            ? Result.Success(new PurchaseRecognitionStartResponse(waiting, true))
            : Result<PurchaseRecognitionStartResponse>.Error(new ErrorList(requested.Errors));
    }
}
