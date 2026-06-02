using Reference.Application.Features.AdditionalReference.GetList.DTOs;

namespace Reference.Application.Features.AdditionalReference.GetList;

public sealed record GetAdditionalReferenceQuery : IQuery<Result<List<AdditionalReferenceRowDTO>>> { }
