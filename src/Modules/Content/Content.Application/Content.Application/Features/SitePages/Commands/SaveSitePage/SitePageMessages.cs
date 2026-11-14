using Content.Domain.SitePages;

using Content.Application.Features.SitePages.Shared;

namespace Content.Application.Features.SitePages.Commands.SaveSitePage;

/// <summary>Texts staff see when «Сторінки сайту» refuses a save.</summary>
public static class SitePageMessages
{
    public const string NotFound = "Такої сторінки сайту немає.";
    public const string Stale = "Сторінку вже змінив інший користувач. Оновіть сторінку.";
    public const string TitleTooLong = "Заголовок задовгий.";
    public const string DescriptionTooLong = "Опис задовгий.";
    public static readonly string TooManyBlocks = $"Текст — не більше {SitePage.MaxBlocks} блоків.";
    public static readonly string TextTooLong = $"Текст — не більше {SitePage.TextMaxLength} символів.";
    public const string UnknownBlock = "Невідомий тип блоку тексту.";
    public static readonly string TooManyQuestions = $"Не більше {SitePage.MaxFaqItems} запитань.";
    public static readonly string QuestionTooLong = $"Запитання — не більше {SitePage.QuestionMaxLength} символів.";
    public static readonly string AnswerTooLong = $"Відповідь — не більше {SitePage.AnswerMaxLength} символів.";
}
