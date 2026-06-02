using Reference.Application.Features.AdditionalReference.Create.DTOs;

namespace Reference.Application.Features.AdditionalReference.Create;

public sealed record CreateAdditionalReferenceCommand(CreateAdditionalReferenceCommandRequest Request)
    : ICommand<Result<int>>;
