using System.Text.Json.Serialization;
namespace Catalog.Application.Features.Product.Commands.CreateSewingArticle;

public sealed record CreateSewingArticleRequest(string SaleSku, bool Active, int SortOrder,
    [property: JsonRequired] int FabricId, [property: JsonRequired] decimal FabricQuantity,
    [property: JsonRequired] long ExpectedVersion);
