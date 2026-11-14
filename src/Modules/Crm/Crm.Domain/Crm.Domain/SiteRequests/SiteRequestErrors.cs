using BuildingBlocks.Domain.Errors;

namespace Crm.Domain.SiteRequests;

/// <summary>Business rule errors of requests from the site.</summary>
public static class SiteRequestErrors
{
    public static Error PhoneRequired() =>
        Error.Validation("crm.site_request_phone_required", "Вкажіть телефон.", "phone");

    public static Error CloseStatusInvalid() =>
        Error.Validation("crm.site_request_close_status", "Заявку закривають як «Готово» або «Не заявка».", "action");
}
