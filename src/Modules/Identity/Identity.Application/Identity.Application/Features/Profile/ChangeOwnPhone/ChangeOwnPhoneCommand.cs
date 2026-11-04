namespace Identity.Application.Features.Profile.ChangeOwnPhone;

/// <summary>Empty or null clears the phone number.</summary>
public sealed record ChangeOwnPhoneCommand(string? PhoneNumber) : ICommand<Result>;
