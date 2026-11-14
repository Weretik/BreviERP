using BuildingBlocks.Domain.Errors;

namespace Content.Domain.SitePages;

/// <summary>Business rule errors of the site pages.</summary>
public static class SitePageErrors
{
    public static Error TextTooLong() =>
        Error.Validation("content.page_text_too_long", "Текст сторінки задовгий.", "Text");

    public static Error FaqTooLong() =>
        Error.Validation("content.page_faq_too_long", "Забагато питань або вони задовгі.", "Faq");

    public static Error TitleTooLong() =>
        Error.Validation("content.page_title_too_long", "Заголовок задовгий.", "Title");

    public static Error DescriptionTooLong() =>
        Error.Validation("content.page_description_too_long", "Опис задовгий.", "Description");
}
