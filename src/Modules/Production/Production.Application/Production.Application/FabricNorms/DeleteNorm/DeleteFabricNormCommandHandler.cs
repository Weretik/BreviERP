using Production.Application.Abstractions;
using Production.Domain.FabricNorms;

namespace Production.Application.FabricNorms.DeleteNorm;

public sealed class DeleteFabricNormCommandHandler(IProductionDbContext db) : ICommandHandler<DeleteFabricNormCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteFabricNormCommand command, CancellationToken cancellationToken)
    {
        var code = ModelFabricNorm.NormalizeCode(command.ModelCode);
        var norm = await db.FabricNorms.FirstOrDefaultAsync(x => x.ModelCode == code, cancellationToken);
        if (norm is null) return Result.NotFound(FabricNormMessages.NormNotFound);
        db.FabricNorms.Remove(norm);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
