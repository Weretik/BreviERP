namespace Catalog.Application.Contracts.SlugGeneration;

public interface IProductSlugGenerator
{
    string GenerateFromUkrainianName(string name);
}
