using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Orders.Shared;

/// <summary>What the manager or the head of the workshop fill in for an item.</summary>
public sealed record ProductionItemDetailsInput(
    int Quantity,
    DateOnly DueDate,
    string MainColor,
    IReadOnlyList<string> AdditionalColors,
    string? SizesText,
    string? TechnicalChanges,
    string? Comment,
    IReadOnlyList<ItemApplicationInput> Applications);
