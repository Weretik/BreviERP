namespace Catalog.Application.Features.Product.Sewing;

public sealed record ReplaceSewingCompositionCommand(int ProductId, ReplaceSewingCompositionRequest Request, string Actor) : ICommand<Result<SewingComposition>>;

