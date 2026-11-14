namespace Reference.Application.Features.PricingParameter.Commands.DeletePricingParameter;

public sealed record DeletePricingParameterCommand(int Id) : ICommand<Result>;
