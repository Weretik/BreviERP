
namespace Reference.Application.Features.PricingParameter.Commands.CreatePricingParameter;

public sealed record CreatePricingParameterCommand(CreatePricingParameterCommandRequest Request) : ICommand<Result<int>>;
