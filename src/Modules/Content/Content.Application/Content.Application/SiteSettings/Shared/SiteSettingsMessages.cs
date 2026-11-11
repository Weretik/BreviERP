namespace Content.Application.SiteSettings.Shared;

/// <summary>Texts staff see when «Налаштування SEO сайту» refuses a save.</summary>
public static class SiteSettingsMessages
{
    public const string Stale = "Налаштування вже змінив інший користувач. Оновіть сторінку.";
    public const string AnalyticsId = "Google Analytics 4: ідентифікатор потоку виду G-XXXXXXXXXX.";
    public const string TagManagerId = "Google Tag Manager: ідентифікатор контейнера виду GTM-XXXXXXX.";
    public const string Verification = "Код підтвердження: лише значення content з мета-тегу (літери, цифри, - і _), від 8 до 100 символів.";
    public const string PhotoNotFound = "Обраного фото немає в бібліотеці «Фото». Оберіть інше.";
}
