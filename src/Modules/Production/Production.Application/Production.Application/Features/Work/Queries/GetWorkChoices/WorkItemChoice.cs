using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Work.Queries.GetWorkChoices;

/// <summary>An item of a «Нове» order that work can be recorded for.</summary>
public sealed record WorkItemChoice(int OrderId, int OrderNumber, string ClientName, int ItemId, string ModelName,
    int Quantity, int Sewn, int Cut, decimal? SewingRate);
