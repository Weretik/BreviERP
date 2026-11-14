using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Commands.SetEmbroideryMockup;

public sealed class SetEmbroideryMockupValidator : AbstractValidator<SetEmbroideryMockupCommand>
{
    public SetEmbroideryMockupValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.MediaFileId).GreaterThan(0).When(x => x.Input is { MediaFileId: not null });
    }
}
