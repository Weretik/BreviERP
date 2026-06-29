namespace Catalog.Application.Features.Media.Delete;

public sealed record DeleteMediaFileCommand(int Id) : ICommand<Result>;
