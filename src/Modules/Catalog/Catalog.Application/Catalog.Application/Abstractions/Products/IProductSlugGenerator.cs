namespace Catalog.Application.Abstractions.Products;

public interface IProductSlugGenerator
{
    string GenerateFromUkrainianName(string name);
}
