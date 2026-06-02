using Reference.Application.Features.AdditionalReference.Update.DTOs;

namespace Reference.Application.Features.AdditionalReference.Update;

public sealed record UpdateAdditionalReferenceCommand(int Id, UpdateAdditionalReferenceCommandRequest Request)
    : ICommand<Result>;
