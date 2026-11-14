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
        if (ModelFabricNorm.CheckCode(command.ModelCode) is { IsFailure: true } badCode)
            return badCode.ToResult<FabricNormRowResponse>();
        var code = ModelFabricNorm.NormalizeCode(command.ModelCode);
        var model = (await models.ListSewingModelsAsync(cancellationToken)).FirstOrDefault(x => x.Code == code);
        if (model is null) return Result<FabricNormRowResponse>.NotFound(FabricNormMessages.ModelNotFound);

        var values = command.Norm.ToValues();
        var now = clock.GetUtcNow();
        var norm = await db.FabricNorms.FirstOrDefaultAsync(x => x.ModelCode == code, cancellationToken);
        if (norm is null)
        {
            var created = ModelFabricNorm.Create(code, values, command.UserId, now);
            if (created.IsFailure) return created.Error!.ToResult<FabricNormRowResponse>();
            db.FabricNorms.Add(norm = created.Value);
        }
        else if (norm.Set(values, command.UserId, now) is { IsFailure: true } set)
            return set.Error!.ToResult<FabricNormRowResponse>();
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new FabricNormRowResponse(code, model.Name, model.IsActive, norm.Values.ToInput(),
            norm.Values.TwoColorsTotal, norm.Values.ThreeColorsTotal, norm.UpdatedAt, null, model.ProductId, model.Season));
    }
}
