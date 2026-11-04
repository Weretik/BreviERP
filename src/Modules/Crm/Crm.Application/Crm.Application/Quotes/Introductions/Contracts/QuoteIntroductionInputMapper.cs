using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Introductions.Contracts;

internal static class QuoteIntroductionInputMapper
{
    /// <summary>An introduction has no client comment: the letter and the catalogue speak to the client.</summary>
    public static QuoteHeader ToHeader(this SaveQuoteIntroductionInput input) =>
        new(input.InternalName, input.CustomerId, input.RecipientName, input.CompanyName, null, input.ManagerNote);

    public static QuoteIntroduction ToIntroduction(this SaveQuoteIntroductionInput input) =>
        new(Enum.Parse<QuoteIntroLayout>(input.Layout), input.RecipientEmail, input.LetterName);
}
