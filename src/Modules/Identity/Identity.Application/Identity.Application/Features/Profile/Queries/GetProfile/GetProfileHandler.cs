using Identity.Application.Features.Profile.Shared;
using Identity.Application.Abstractions;

namespace Identity.Application.Features.Profile.Queries.GetProfile;

public sealed class GetProfileHandler(IProfileService profileService)
    : IQueryHandler<GetProfileQuery, Result<ProfileDto>>
{
    public async ValueTask<Result<ProfileDto>> Handle(GetProfileQuery query, CancellationToken cancellationToken)
        => await profileService.GetAsync(cancellationToken);
}
