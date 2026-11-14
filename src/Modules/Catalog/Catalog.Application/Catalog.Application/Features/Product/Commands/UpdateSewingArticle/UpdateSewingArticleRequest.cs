using System.Text.Json.Serialization;
namespace Catalog.Application.Features.Product.Commands.UpdateSewingArticle;

public sealed record UpdateSewingArticleRequest(string SaleSku, bool Active,
    [property: JsonRequired] int FabricId, [property: JsonRequired] decimal FabricQuantity,
    [property: JsonRequired] long ExpectedVersion);
