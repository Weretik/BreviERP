using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.Photos;

/// <summary>
/// «Змінити фото → Завантажити своє» (R-34): the photo goes to the CDN folder of КП photos and only into the item that
/// the manager saves it in; it is not added to the product or offered in other КП.
/// </summary>
public sealed record UploadQuotePhotoCommand(Stream Content, string FileName, string ContentType, long SizeInBytes)
    : ICommand<Result<QuotePhotoResponse>>;
