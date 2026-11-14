using Crm.Domain.Embroidery.Saved;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryMockupUsages;

public sealed class GetEmbroideryMockupUsagesValidator : AbstractValidator<GetEmbroideryMockupUsagesQuery>
{
    public GetEmbroideryMockupUsagesValidator() => RuleFor(x => x.MediaFileIds).NotNull();
}
