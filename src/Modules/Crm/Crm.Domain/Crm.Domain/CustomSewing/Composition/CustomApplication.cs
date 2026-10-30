namespace Crm.Domain.CustomSewing.Composition;

public sealed record CustomApplication(string Kind, string Location, Guid? ArtworkFileId,
    decimal Cost, decimal SaleAmount, bool SaleApproved);
