using Identity.Application.Security.Policies;
using Identity.Api.Endpoints.Profile.ChangeMyEmail;
using Identity.Api.Endpoints.Profile.ChangeMyName;
using Identity.Api.Endpoints.Profile.ChangeMyPassword;
using Identity.Api.Endpoints.Profile.ChangeMyPhone;
using Identity.Api.Endpoints.Profile.GetMyProfile;
using Identity.Api.Endpoints.Profile.RemoveMyAvatar;
using Identity.Api.Endpoints.Profile.UploadMyAvatar;

namespace Identity.Api.Endpoints.Profile;

/// <summary>The signed-in staff account's own profile («Мій кабінет»). Every staff member changes name and photo;
/// administrators (and the director) also change phone, email and password (the last two with the current password).</summary>
internal static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/profile").RequireAuthorization(PolicyNames.OwnProfile).WithTags("Profile");
        GetMyProfileEndpoint.Map(group);
        ChangeMyNameEndpoint.Map(group);
        ChangeMyPhoneEndpoint.Map(group);
        UploadMyAvatarEndpoint.Map(group);
        RemoveMyAvatarEndpoint.Map(group);
        ChangeMyEmailEndpoint.Map(group);
        ChangeMyPasswordEndpoint.Map(group);
        return app;
    }
}
