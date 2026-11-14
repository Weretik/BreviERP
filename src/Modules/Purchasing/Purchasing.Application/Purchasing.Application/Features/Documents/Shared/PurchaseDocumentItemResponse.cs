using System.Text.Json.Serialization;

namespace Purchasing.Application.Features.Documents.Shared;

public sealed record PurchaseDocumentItemResponse(int Position, string Name, string Quantity, string? Unit, string Price, string Sum);
