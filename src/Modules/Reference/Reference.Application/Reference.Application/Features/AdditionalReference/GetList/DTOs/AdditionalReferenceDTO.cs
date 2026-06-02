namespace Reference.Application.Features.AdditionalReference.GetList.DTOs;

public sealed record AdditionalReferenceRowDTO(
    int Id,
    string Name,
    string Key,
    decimal Value,
    string Unit,
    string? Description);
