using Microsoft.AspNetCore.Http;

namespace Crm.Api.Quotes.Requests;

public sealed record UploadQuotePhotoRequest(IFormFile? File);
