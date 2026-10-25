using System.Text.Json.Serialization;
namespace Catalog.Application.Features.Product.Sewing;

public sealed record CreateSewingArticleRequest(string SaleSku, bool Active, int SortOrder, [property: JsonRequired] long ExpectedVersion);

