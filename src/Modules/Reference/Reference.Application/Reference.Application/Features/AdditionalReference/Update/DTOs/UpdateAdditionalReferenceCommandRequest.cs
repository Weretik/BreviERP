namespace Reference.Application.Features.AdditionalReference.Update.DTOs;

public sealed record UpdateAdditionalReferenceCommandRequest(
    string Name,
    string Key,
    decimal Value,
    string Unit,
    string? Description);
