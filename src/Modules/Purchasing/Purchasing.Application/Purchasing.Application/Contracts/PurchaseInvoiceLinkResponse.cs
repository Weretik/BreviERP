namespace Purchasing.Application.Contracts;

/// <summary>An original-document link; <see cref="Safe"/> is false for anything but an HTTPS Dropbox link.</summary>
public sealed record PurchaseInvoiceLinkResponse(string Id, string Url, bool Safe);
