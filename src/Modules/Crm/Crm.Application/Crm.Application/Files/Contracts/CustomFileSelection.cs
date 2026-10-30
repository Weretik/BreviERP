namespace Crm.Application.Files.Contracts;

public sealed record CustomFileSelection(Guid FileId, bool ShowInQuote, bool Production);
