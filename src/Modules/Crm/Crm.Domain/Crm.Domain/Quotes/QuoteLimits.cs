namespace Crm.Domain.Quotes;

public static class QuoteLimits
{
    public const int NameMaxLength = 200;
    public const int TextMaxLength = 4000;
    public const int UrlMaxLength = 1000;
    public const int CodeMaxLength = 100;
    public const int ReasonMaxLength = 500;
    public const int NumberMaxLength = 20;
    public const int EmailMaxLength = 320;
    public const int LetterNameMaxLength = 300;
    public const int IntroModelsPerPage = 4;
    public const int MaxItems = 100;
    public const int MaxOptionsPerItem = 20;
    public const decimal MaxPrice = 10_000_000m;
    public const int MaxCompositionLines = 200;
    public const decimal MaxCompositionQuantity = 10_000m;
    public const int UnitMaxLength = 20;
    public const int CostCodeMaxLength = 80;
}
