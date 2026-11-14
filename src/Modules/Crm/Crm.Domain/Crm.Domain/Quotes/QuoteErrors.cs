using BuildingBlocks.Domain.Errors;
using Crm.Domain.Quotes.Settings;

namespace Crm.Domain.Quotes;

/// <summary>Business rule errors of КП, their settings and introduction templates.</summary>
public static class QuoteErrors
{
    public static Error InternalNameRequired() =>
        Error.Validation("crm.quote_internal_name_required", "Вкажіть внутрішню назву клієнта або запиту.", "internalName");

    public static Error TooManyItems() =>
        Error.Conflict("crm.quote_too_many_items", "У КП вже максимум позицій.");

    public static Error NotIntroduction() =>
        Error.Conflict("crm.quote_not_introduction", "Сторінки знайомства є лише в «КП для знайомства».");

    public static Error CompanyNameRequired() =>
        Error.Validation("crm.quote_settings_company_required", "Вкажіть назву компанії.", "company.name");

    public static Error AdvantagesCount() =>
        Error.Validation("crm.quote_settings_advantages",
            $"Переваг у каталозі знайомства має бути рівно {QuoteSettingsLimits.AdvantagesCount}.", "introduction.advantages");

    public static Error StaffContactRepeated() =>
        Error.Validation("crm.quote_settings_staff_repeated", "Для кожного працівника — один контакт.", "staffContacts");

    public static Error TemplateNameRequired() =>
        Error.Validation("crm.quote_template_name_required", "Вкажіть назву шаблону.", "name");
}
