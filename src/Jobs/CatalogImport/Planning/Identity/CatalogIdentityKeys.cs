namespace CatalogImport.Planning;

public sealed record CatalogIdentityKeys(
    HashSet<string> Models, HashSet<string> Ppe, HashSet<string> Articles,
    HashSet<string> Fabrics, HashSet<string> Accessories, HashSet<string> Operations);

