using BuildingBlocks.Domain.Errors;

namespace Crm.Domain.Customers;

/// <summary>Business rule errors of clients, their contacts and external identities.</summary>
public static class CustomerErrors
{
    public const int MaxContacts = 100;
    public const int MaxChannels = 10;
    public const int MaxExternalIdentities = 30;

    public static Error Stale() =>
        Error.Conflict("crm.customer_stale", "Клієнта вже змінили. Оновіть сторінку й повторіть зміни.");

    public static Error Required(string field) =>
        Error.Validation("crm.customer_required", "Заповніть це поле.", field);

    public static Error TooLong(string field, int max) =>
        Error.Validation("crm.customer_too_long", $"Не довше {max} символів.", field);

    public static Error KindInvalid() =>
        Error.Validation("crm.customer_kind_invalid", "Виберіть: організація або особа.", "kind");

    public static Error ContactNotFound() =>
        Error.NotFound("crm.contact_not_found", "Контакт не знайдено. Оновіть сторінку.");

    public static Error TooManyContacts() =>
        Error.Validation("crm.contacts_too_many", $"У клієнта може бути не більше {MaxContacts} контактів.", "contacts");

    public static Error ChannelsDuplicateOrTooMany() =>
        Error.Validation("crm.contact_channels_invalid",
            $"Телефони й пошти контакту не повторюються, їх не більше {MaxChannels}.", "channels");

    public static Error ChannelKindInvalid() =>
        Error.Validation("crm.channel_kind_invalid", "Виберіть: телефон або пошта.", "channels");

    public static Error EmailInvalid() =>
        Error.Validation("crm.channel_email_invalid", "Перевірте адресу пошти.", "channels");

    public static Error PhoneInvalid() =>
        Error.Validation("crm.channel_phone_invalid", "Телефон — від 5 до 20 цифр.", "channels");

    public static Error ExternalIdentityDuplicateOrTooMany() =>
        Error.Conflict("crm.external_identity_conflict",
            $"Такий зовнішній ідентифікатор уже є, або їх більше {MaxExternalIdentities}.");

    public static Error MergeSameCustomer() =>
        Error.Validation("crm.merge_same_customer", "Не можна об'єднати клієнта із самим собою.", "victimId");

    public static Error MergeSurvivorInactive() =>
        Error.Conflict("crm.merge_survivor_inactive", "Клієнт, до якого приєднують, має бути активним.");

    public static Error MergeExternalIdentityConflict() =>
        Error.Conflict("crm.merge_identity_conflict", "В обох клієнтів однаковий зовнішній ідентифікатор.");

    public static Error MergeLimitsExceeded() =>
        Error.Conflict("crm.merge_limits_exceeded",
            $"Після об'єднання буде більше {MaxContacts} контактів або {MaxExternalIdentities} зовнішніх ідентифікаторів.");
}
