namespace Reference.Application.Features.PricingParameter.Delete;

public sealed record DeletePricingParameterCommand(int Id) : ICommand<Result>;
