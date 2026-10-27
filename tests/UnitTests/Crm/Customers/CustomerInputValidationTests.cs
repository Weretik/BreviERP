using Crm.Application.Customers.SaveCustomerContact.Inputs;
using Crm.Application.Customers.Contracts.Inputs;
using Crm.Application.Customers.Access;
using Crm.Application.Customers.CreateCustomer;
using Crm.Application.Customers.SaveCustomerContact;

namespace UnitTests.Crm.Customers;

public sealed class CustomerInputValidationTests
{
    [Test]
    public void NullExternalIdentityIsRejectedBeforeTheHandler()
    {
        var command = new CreateCustomerCommand(new CustomerInput("person", "Synthetic", null, null, [null!]), Guid.NewGuid(), new CrmActor(17, false));
        Assert.That(new CreateCustomerCommandValidator().Validate(command).IsValid, Is.False);
    }

    [Test]
    public void NullContactChannelIsRejectedBeforeTheHandler()
    {
        var command = new SaveCustomerContactCommand(Guid.NewGuid(), null, 1, new ContactInput("Synthetic", null, true, false, [null!]), Guid.NewGuid(), new CrmActor(17, false));
        Assert.That(new SaveCustomerContactCommandValidator().Validate(command).IsValid, Is.False);
    }
}
