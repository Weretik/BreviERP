using Ardalis.Result;


using Media.Contracts.Files;
using Crm.Application.Features.Quotes.Commands.UploadQuotePhoto;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Infrastructure.Adapters.Media;

/// <summary>A photo uploaded in a КП goes to the media library folder «Фото КП» and its CDN path (R-34).</summary>
internal sealed class QuotePhotoStorage(IMediaFileUploader uploader) : IQuotePhotoStorage
{
    public const string FolderName = "Фото КП";

    public async Task<Result<string>> StoreAsync(Stream content, string fileName, string contentType,
        long sizeInBytes, CancellationToken cancellationToken)
    {
        var folder = await uploader.EnsureFolderAsync([FolderName], cancellationToken);
        if (!folder.IsSuccess) return Result<string>.Error("Не вдалося підготувати папку «Фото КП».");
        var uploaded = await uploader.UploadAsync(
            new MediaUpload(content, fileName, contentType, sizeInBytes, folder.Value), cancellationToken);
        if (uploaded.IsSuccess) return Result.Success(uploaded.Value.PublicUrl);
        var reason = uploaded.ValidationErrors.Select(x => x.ErrorMessage).Concat(uploaded.Errors).FirstOrDefault();
        return Result<string>.Error(reason ?? "Медіатека не прийняла фото.");
    }
}
