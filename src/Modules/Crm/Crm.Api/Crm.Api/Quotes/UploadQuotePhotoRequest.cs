using Microsoft.AspNetCore.Http;

namespace Crm.Api.Quotes;

public sealed record UploadQuotePhotoRequest(IFormFile? File);
