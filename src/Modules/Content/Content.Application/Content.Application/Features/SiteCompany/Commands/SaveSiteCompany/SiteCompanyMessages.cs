using Content.Application.Features.SiteCompany.Shared;

namespace Content.Application.Features.SiteCompany.Commands.SaveSiteCompany;

/// <summary>Texts staff see when «Контакти та компанія» refuses a save.</summary>
public static class SiteCompanyMessages
{
    public const string Stale = "Дані вже змінив інший користувач. Оновіть сторінку.";
    public const string Phone = "Телефон: український номер, наприклад 050 469 86 75.";
    public const string Email = "Пошта: адреса виду name@example.com.";
    public const string Required = "Заповніть українською й російською.";
    public const string TooLong = "Текст задовгий: не більше 200 символів.";
    public const string Days = "Оберіть хоча б один робочий день.";
    public const string Time = "Час виду 09:00; кінець пізніше за початок.";
    public const string Managers = "Не більше 10 менеджерів.";
    public const string ManagerPhones = "У менеджера від одного до трьох телефонів.";
    public const string Photo = "Фото: оберіть з бібліотеки «Фото».";
    public const string Edrpou = "ЄДРПОУ: 8 цифр (або 10 цифр для ФОП).";
    public const string Iban = "IBAN: UA і 27 цифр.";
    public const string Link = "Посилання: повна адреса, що починається з https://.";
}
