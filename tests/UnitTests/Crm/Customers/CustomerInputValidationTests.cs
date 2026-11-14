using Crm.Application.Features.Customers.Commands.CreateCustomer;
using Crm.Application.Features.Customers.Commands.SaveCustomerContact;
using Crm.Application.Features.Customers.Shared.Inputs;
using Crm.Application.Abstractions.Customers;

namespace UnitTests.Crm.Customers;

public sealed class CustomerInputValidationTests
{
    [Test]
    public void NullExternalIdentityIsRejectedBeforeTheHandler()
    {
        var command = new CreateCustomerCommand(new CustomerInput("person", "Synthetic", null, null, [null!]), Guid.NewGuid(), new CrmActor(17, false));
        Assert.That(new CreateCustomerValidator().Validate(command).IsValid, Is.False);
    }

    [Test]
    public void NullContactChannelIsRejectedBeforeTheHandler()
    {
        var command = new SaveCustomerContactCommand(Guid.NewGuid(), null, 1, new ContactInput("Synthetic", null, true, false, [null!]), Guid.NewGuid(), new CrmActor(17, false));
        Assert.That(new SaveCustomerContactValidator().Validate(command).IsValid, Is.False);
    }
}
