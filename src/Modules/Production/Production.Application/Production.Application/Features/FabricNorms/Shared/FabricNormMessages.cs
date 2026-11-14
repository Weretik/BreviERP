namespace Production.Application.Features.FabricNorms.Shared;

internal static class FabricNormMessages
{
    public const string ModelNotFound = "Модель не знайдено в «Товари».";
    public const string ModelInactive = "Модель вимкнена в «Товари».";
    public const string NormNotFound = "Норму для цієї моделі не знайдено.";
    public const string ValueInvalid = "Норма — додатне число метрів, не більше 100, до трьох знаків після коми.";
    public const string AdditionalWithoutMain = "Вкажіть основну тканину, якщо задана додаткова.";
    public const string Empty = "Вкажіть хоча б одну норму.";
}
