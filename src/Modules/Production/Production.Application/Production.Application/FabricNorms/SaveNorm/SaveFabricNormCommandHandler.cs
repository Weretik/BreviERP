using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Domain.FabricNorms;

namespace Production.Application.FabricNorms.SaveNorm;

public sealed class SaveFabricNormCommandHandler(IProductionDbContext db, IProductionModelReader models, TimeProvider clock)
    : ICommandHandler<SaveFabricNormCommand, Result<FabricNormRowResponse>>
{
    public async ValueTask<Result<FabricNormRowResponse>> Handle(SaveFabricNormCommand command,
        CancellationToken cancellationToken)
    {
        var code = ModelFabricNorm.NormalizeCode(command.ModelCode);
        var model = (await models.ListSewingModelsAsync(cancellationToken)).FirstOrDefault(x => x.Code == code);
        if (model is null) return Result<FabricNormRowResponse>.NotFound(FabricNormMessages.ModelNotFound);

        var values = command.Norm.ToValues();
        var now = clock.GetUtcNow();
        var norm = await db.FabricNorms.FirstOrDefaultAsync(x => x.ModelCode == code, cancellationToken);
        if (norm is null) db.FabricNorms.Add(norm = new ModelFabricNorm(code, values, command.UserId, now));
        else norm.Set(values, command.UserId, now);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new FabricNormRowResponse(code, model.Name, model.IsActive, norm.Values.ToInput(),
            norm.Values.TwoColorsTotal, norm.Values.ThreeColorsTotal, norm.UpdatedAt, null));
    }
}
