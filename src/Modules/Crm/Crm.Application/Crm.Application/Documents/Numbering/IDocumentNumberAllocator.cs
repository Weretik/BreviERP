namespace Crm.Application.Documents.Numbering;

public interface IDocumentNumberAllocator
{
    // Called in the same Crm transaction that moves the quote to ready.
    Task<string> AllocateAsync(DateTimeOffset readyAt, CancellationToken ct);
}
