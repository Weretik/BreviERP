using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Domain.Quotes.ValueObjects;

namespace Crm.Application.Features.Quotes.Shared.Introductions;

internal static class QuoteIntroductionInputMapper
{
    /// <summary>An introduction has no client comment: the letter and the catalogue speak to the client.</summary>
    public static QuoteHeader ToHeader(this SaveQuoteIntroductionInput input, QuoteCustomer? customer) =>
        new(input.InternalName, customer, input.RecipientName, input.CompanyName, null, input.ManagerNote);

    public static QuoteIntroduction ToIntroduction(this SaveQuoteIntroductionInput input) =>
        new(Enum.Parse<QuoteIntroLayout>(input.Layout), input.RecipientEmail, input.LetterName, input.LetterSubject,
            input.LetterBody);
}
