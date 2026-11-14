namespace Production.Application.Features.PayrollParameters.Shared;

/// <summary>Codes of «Параметри ціноутворення» that the workshop pays by, and what each one is for.</summary>
public static class PayrollParameterCodes
{
    public const string SeamstressBonus = "SEAMSTRESS_BONUS";
    public const string SeamstressBonusThreshold = "SEAMSTRESS_BONUS_PAYOUT_THRESHOLD";
    public const string CuttingRate = "CUTTING_RATE";
    public const string MasterRate = "MASTER_RATE";
    public const string ShopManagerRate = "SHOP_MANAGER_RATE";
    public const string EmbroideryRate = "EMBROIDERY_RATE_1000_STITCHES";
    public const string SeamstressCount = "SEAMSTRESS_COUNT";
    public const string MinuteCost = "MINUTE_COST_SEAMSTRESS";
    public const string ShiftMinutes = "MINUTES_SHIFT";

    public static IReadOnlyList<(string Code, string Purpose)> All { get; } =
    [
        (SeamstressBonus, "Премія швачки: частка від суми за пошиття"),
        (SeamstressBonusThreshold, "Премія швачки: з якої суми за пошиття за місяць нараховується"),
        (CuttingRate, "Оплата крою: частка від розцінки пошиття за кожен покроєний виріб"),
        (MasterRate, "Майстер цеху: частка від загальної суми за пошиття всіх швачок"),
        (ShopManagerRate, "Начальник цеху: частка від загальної суми за пошиття всіх швачок"),
        (EmbroideryRate, "Вишивка: оплата за 1000 стібків"),
        (SeamstressCount, "Прогноз пошиття: кількість швачок"),
        (ShiftMinutes, "Прогноз пошиття: хвилин у зміні"),
        (MinuteCost, "Прогноз пошиття: вартість хвилини роботи швачки"),
    ];
}
