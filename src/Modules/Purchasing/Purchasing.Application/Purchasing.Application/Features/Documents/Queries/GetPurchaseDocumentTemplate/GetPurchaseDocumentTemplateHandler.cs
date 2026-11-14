using System.Text.Json;
using System.Text.Json.Nodes;
using Purchasing.Application.Abstractions;
using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Application.Features.Documents.Shared.Recognition;

namespace Purchasing.Application.Features.Documents.Queries.GetPurchaseDocumentTemplate;

public sealed class GetPurchaseDocumentTemplateHandler(IPurchasingDbContext db)
    : IQueryHandler<GetPurchaseDocumentTemplateQuery, Result<string>>
{
    private const string Empty = """
        {"documentType":"","documentNumber":"","documentDate":"","supplierName":"","vat":"without_vat","pricesIncludeVat":false,
         "items":[{"name":"","quantity":"1","unit":null,"price":"0.00","sum":"0.00"}],"vatAmount":null,"totalPayable":"0.00"}
        """;

    public async ValueTask<Result<string>> Handle(GetPurchaseDocumentTemplateQuery query, CancellationToken cancellationToken)
    {
        var file = await db.Files.AsNoTracking().FirstOrDefaultAsync(x => x.Id == query.FileId && x.Active, cancellationToken);
        if (file is null) return Result<string>.NotFound(PurchaseFileRecognition.FileNotFound);
        var uid = await db.SourceRows.Where(x => x.Active && x.MessageId == file.MessageId)
            .OrderBy(x => x.RowNumber).ThenBy(x => x.Id).Select(x => x.SourceUid).FirstOrDefaultAsync(cancellationToken);
        if (uid is null) return Result<string>.NotFound(PurchaseFileRecognition.NoSourceRow);

        var saved = await db.CheckedDocuments.AsNoTracking().Where(x => x.Id == file.Id).Select(x => x.PayloadJson)
            .FirstOrDefaultAsync(cancellationToken);
        var output = saved is null
            ? await db.RecognitionAttempts.AsNoTracking().Where(x => x.FileId == file.Id && x.OutputJson != null)
                .OrderByDescending(x => x.Id).Select(x => x.OutputJson).FirstOrDefaultAsync(cancellationToken)
            : null;

        JsonObject body;
        try
        {
            body = JsonNode.Parse(saved ?? output ?? Empty) as JsonObject ?? (JsonObject)JsonNode.Parse(Empty)!;
        }
        catch (JsonException)
        {
            body = (JsonObject)JsonNode.Parse(Empty)!;
        }

        var template = new JsonObject { ["sourceUID"] = uid, ["originalFileLink"] = file.Url };
        foreach (var (key, value) in body.ToArray())
            if (key is not ("sourceUID" or "originalFileLink")) template[key] = value?.DeepClone();
        return Result.Success(template.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        }));
    }
}
