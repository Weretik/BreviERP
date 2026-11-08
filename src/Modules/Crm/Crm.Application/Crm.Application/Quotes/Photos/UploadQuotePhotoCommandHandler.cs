using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.Photos;

public sealed class UploadQuotePhotoCommandHandler(IQuotePhotoStorage storage)
    : ICommandHandler<UploadQuotePhotoCommand, Result<QuotePhotoResponse>>
{
    public async ValueTask<Result<QuotePhotoResponse>> Handle(UploadQuotePhotoCommand command,
        CancellationToken cancellationToken)
    {
        var stored = await storage.StoreAsync(command.Content, command.FileName, command.ContentType.ToLowerInvariant(),
            command.SizeInBytes, cancellationToken);
        return stored.IsSuccess
            ? Result.Success(new QuotePhotoResponse(stored.Value))
            : Result<QuotePhotoResponse>.Invalid(new ValidationError(
                stored.Errors.FirstOrDefault() ?? "Не вдалося завантажити фото."));
    }
}
