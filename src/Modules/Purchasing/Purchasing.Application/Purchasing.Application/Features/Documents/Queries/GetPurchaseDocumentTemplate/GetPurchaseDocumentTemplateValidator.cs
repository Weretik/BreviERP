using System.Text.Json;
using System.Text.Json.Nodes;
using Purchasing.Application.Abstractions;
using Purchasing.Application.Features.Documents.Shared;

namespace Purchasing.Application.Features.Documents.Queries.GetPurchaseDocumentTemplate;

public sealed class GetPurchaseDocumentTemplateValidator : AbstractValidator<GetPurchaseDocumentTemplateQuery>
{
    public GetPurchaseDocumentTemplateValidator() => RuleFor(x => x.FileId).NotEmpty().MaximumLength(64);
}
