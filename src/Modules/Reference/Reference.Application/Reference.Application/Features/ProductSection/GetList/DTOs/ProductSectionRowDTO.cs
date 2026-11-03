namespace Reference.Application.Features.ProductSection.GetList.DTOs;

/// <summary>A product section; <paramref name="Type"/> is <c>Sewing</c> or <c>Ppe</c>.</summary>
public sealed record ProductSectionRowDTO(
    int Id,
    string Type,
    string Name,
    bool IsActive,
    string? Comment,
    int SortOrder);
