namespace Reference.Application.Features.AdditionalReference.Delete;

public sealed record DeleteAdditionalReferenceCommand(int Id) : ICommand<Result>;
