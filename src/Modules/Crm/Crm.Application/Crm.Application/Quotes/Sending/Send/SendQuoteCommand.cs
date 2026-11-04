using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.Sending.Send;

/// <summary>
/// «Надіслати»: sends the PDF of the saved КП from the signed-in person's Gmail (R-15). On success the КП is sent;
/// a failure leaves it as it was and returns the reason (409).
/// </summary>
public sealed record SendQuoteCommand(QuoteActor Actor, long Id, SendQuoteInput Input) : ICommand<Result<QuoteResponse>>;

/// <param name="Version">The КП version the editor shows; the PDF of exactly this version is sent.</param>
/// <param name="To">One or more addresses separated by commas or semicolons.</param>
public sealed record SendQuoteInput(int Version, string To, string Subject, string Body);
