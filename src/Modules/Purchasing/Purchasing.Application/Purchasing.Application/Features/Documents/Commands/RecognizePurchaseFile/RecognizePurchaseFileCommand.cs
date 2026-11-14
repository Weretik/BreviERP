using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Application.Features.Documents.Shared.Recognition;

namespace Purchasing.Application.Features.Documents.Commands.RecognizePurchaseFile;

/// <summary>
/// «Розпізнати» / «Розпізнати знову» of one file. A new checked document replaces the old one, which goes to history.
/// Unavailable when the AI is off, has no key or reached its limit; Conflict while the sheet sync runs.
/// </summary>
public sealed record RecognizePurchaseFileCommand(string FileId, string By) : ICommand<Result<PurchaseRecognitionResponse>>;
