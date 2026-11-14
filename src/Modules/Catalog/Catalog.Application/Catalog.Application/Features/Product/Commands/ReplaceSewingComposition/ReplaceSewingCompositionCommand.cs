using Catalog.Application.Features.Product.Shared.Sewing;
namespace Catalog.Application.Features.Product.Commands.ReplaceSewingComposition;

public sealed record ReplaceSewingCompositionCommand(int ProductId, ReplaceSewingCompositionRequest Request, string Actor) : ICommand<Result<SewingComposition>>;

