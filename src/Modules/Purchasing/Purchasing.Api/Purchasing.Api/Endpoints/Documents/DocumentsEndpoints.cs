using Purchasing.Api.Access;
using Purchasing.Api.Endpoints.Documents.GetPurchaseDocumentTemplate;
using Purchasing.Api.Endpoints.Documents.RecognizeNewPurchaseFiles;
using Purchasing.Api.Endpoints.Documents.RecognizePurchaseFile;
using Purchasing.Api.Endpoints.Documents.SavePurchaseDocument;

namespace Purchasing.Api.Endpoints.Documents;

/// <summary>The invoice files of «Рахунки на закупку»: AI recognition and the checked facts.</summary>
internal static class DocumentsEndpoints
{
    public static IEndpointRouteBuilder MapDocumentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/purchasing/invoices").RequireAuthorization(PurchasingAccess.Policy)
            .WithTags("PurchaseInvoices");
        RecognizePurchaseFileEndpoint.Map(group);
        RecognizeNewPurchaseFilesEndpoint.Map(group);
        SavePurchaseDocumentEndpoint.Map(group);
        GetPurchaseDocumentTemplateEndpoint.Map(group);
        return app;
    }
}
