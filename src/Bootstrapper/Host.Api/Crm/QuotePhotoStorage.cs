using Ardalis.Result;
using Crm.Application.Quotes.Photos;
using Mediator;
using Media.Application.Files.UploadFile;
using Media.Application.Folders.EnsureFolderPath;

namespace Host.Api.Crm;

/// <summary>A photo uploaded in a КП goes to the media library folder «Фото КП» and its CDN path (R-34).</summary>
internal sealed class QuotePhotoStorage(ISender sender) : IQuotePhotoStorage
{
    public const string FolderName = "Фото КП";

    public async Task<Result<string>> StoreAsync(Stream content, string fileName, string contentType,
        long sizeInBytes, CancellationToken cancellationToken)
    {
        var folder = await sender.Send(new EnsureMediaFolderPathCommand([FolderName]), cancellationToken);
        if (!folder.IsSuccess) return Result<string>.Error("Не вдалося підготувати папку «Фото КП».");
        var uploaded = await sender.Send(
            new UploadMediaFileCommand(content, fileName, contentType, sizeInBytes, folder.Value), cancellationToken);
        if (uploaded.IsSuccess) return Result.Success(uploaded.Value.PublicUrl);
        var reason = uploaded.ValidationErrors.Select(x => x.ErrorMessage).Concat(uploaded.Errors).FirstOrDefault();
        return Result<string>.Error(reason ?? "Медіатека не прийняла фото.");
    }
}
