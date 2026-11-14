using Ardalis.Result;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Host.Seed.Jobs;

/// <summary>Sends a job step's command to its module; a failed result fails the step with the module's reason.</summary>
internal static class JobCommands
{
    public static async Task<T> SendAsync<T>(this IServiceProvider services, ICommand<Result<T>> command,
        CancellationToken cancellationToken)
    {
        var result = await services.GetRequiredService<ISender>().Send(command, cancellationToken);
        if (result.IsSuccess) return result.Value;
        var reasons = result.Errors.Concat(result.ValidationErrors.Select(x => x.ErrorMessage)).ToArray();
        throw new InvalidOperationException(reasons.Length == 0 ? result.Status.ToString() : string.Join(" ", reasons));
    }
}
