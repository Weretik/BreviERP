using Ai.Application.Abstractions;
using Ai.Application.Settings;
using Purchasing.Application.Documents.Recognize;

namespace Host.Api.Purchasing;

/// <summary>Shows the fixed invoice instruction of Purchasing in «Налаштування ШІ» without letting anyone edit it.</summary>
internal sealed class PurchaseInvoiceBasePrompt : IAiBasePrompt
{
    public string Key => AiPromptKeys.PurchaseInvoices;

    public string Text => PurchaseRecognitionPrompt.Instructions;
}
