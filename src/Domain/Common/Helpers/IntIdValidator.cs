namespace Domain.Common.Helpers;

public static class IntIdValidator
{
    public static Validation ValidateIntId(int value, string? name = null)
    {
        if (value <= 0)
            return Validation.Invalid($"{name ?? "Id"} must be positive.");

        if (value > 1_000_000_000)
            return Validation.Invalid($"{name ?? "Id"} is too large.");

        return Validation.Ok;
    }
}
