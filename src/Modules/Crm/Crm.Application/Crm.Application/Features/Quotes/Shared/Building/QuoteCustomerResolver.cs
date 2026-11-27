using Crm.Application.Abstractions.Quotes;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Domain.Quotes.ValueObjects;

namespace Crm.Application.Features.Quotes.Shared.Building;

/// <summary>The client chosen in «Кому» (CARD-001 R-02): an active customer, or nothing for a name typed by hand.</summary>
internal static class QuoteCustomerResolver
{
    public static async Task<Result<QuoteCustomer?>> ResolveCustomerAsync(this IQuoteStore store, Guid? publicId,
        CancellationToken cancellationToken)
    {
        if (publicId is not { } id) return Result<QuoteCustomer?>.Success(null);
        var customer = await store.FindQuoteCustomerAsync(id, cancellationToken);
        return customer is null
            ? Result<QuoteCustomer?>.Invalid(new ValidationError(nameof(SaveQuoteInput.CustomerPublicId), QuoteMessages.CustomerNotFound))
            : Result<QuoteCustomer?>.Success(customer);
    }
}
