using Content.Domain.SiteGalleries;

namespace Content.Application.Features.SiteGalleries.Shared;

/// <summary>Texts staff see when «Наші роботи» or «Сертифікати» refuses a save or a transfer.</summary>
public static class SiteGalleryMessages
{
    public const string NotFound = "Такої сторінки сайту немає.";
    public const string Stale = "Сторінку вже змінив інший користувач. Оновіть сторінку.";
    public static readonly string TooManyWorks = $"Не більше {SiteGallery.MaxWorks} фото.";
    public static readonly string TooManyCertificates = $"Не більше {SiteGallery.MaxCertificates} документів.";
    public const string NoPicture = "У кожного пункту має бути фото з бібліотеки «Фото».";
    public const string UnknownFile = "Фото сайту, якого немає в списку: оберіть фото з бібліотеки «Фото».";
    public const string Twice = "Одне фото додано двічі.";
    public const string NoTitle = "У кожного документа має бути назва українською.";
    public static readonly string TitleTooLong = $"Назва чи підпис: не більше {SiteGallery.TitleMaxLength} символів.";
    public static readonly string NoteTooLong = $"Пояснення: не більше {SiteGallery.NoteMaxLength} символів.";
    public static readonly string Standards =
        $"Стандарти: не більше {SiteGallery.MaxStandards} рядків по {SiteGallery.StandardMaxLength} символів.";
    public const string MissingPhoto = "Фото з бібліотеки не знайдено: його видалили.";
}
