using Ai.Contracts.Prompts;
using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Application.Features.Documents.Shared.Recognition;


namespace Purchasing.Infrastructure.Adapters.Ai;

/// <summary>Shows the fixed invoice instruction of Purchasing in «Налаштування ШІ» without letting anyone edit it.</summary>
internal sealed class PurchaseInvoiceBasePrompt : IAiBasePrompt
{
    public string Key => AiPromptKeys.PurchaseInvoices;

    public string Text => PurchaseRecognitionPrompt.Instructions;
}
