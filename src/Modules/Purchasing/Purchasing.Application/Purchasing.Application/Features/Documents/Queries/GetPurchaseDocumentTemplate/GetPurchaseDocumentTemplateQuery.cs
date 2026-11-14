using System.Text.Json;
using System.Text.Json.Nodes;
using Purchasing.Application.Abstractions;
using Purchasing.Application.Features.Documents.Shared;

namespace Purchasing.Application.Features.Documents.Queries.GetPurchaseDocumentTemplate;

/// <summary>
/// «Шаблон JSON» for a manual fix: the saved facts, else the last AI answer, else empty fields, always with the file's
/// <c>sourceUID</c> and <c>originalFileLink</c> filled in.
/// </summary>
public sealed record GetPurchaseDocumentTemplateQuery(string FileId) : IQuery<Result<string>>;
