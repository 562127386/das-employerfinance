using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerFinance.Application.Commands.UpdateAccountName;
using SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers.EmployerAccounts;
using SFA.DAS.Testing;

namespace SFA.DAS.EmployerFinance.UnitTests.MessageHandlers.EventHandlers.EmployerAccounts
{
    [TestFixture]
    [Parallelizable]
    public class ChangedAccountNameEventHandlerTests : FluentTest<ChangedAccountNameEventHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenHandlingChangedAccountNameEvent_ThenShouldSendUpdateAccountNameCommand()
        {
            return TestAsync(f => f.Handle(), f => f.VerifySend<UpdateAccountNameCommand>((c, m) => 
                c.AccountId == m.AccountId && c.Name == m.CurrentName && c.Updated == m.Created));
        }
    }

    public class ChangedAccountNameEventHandlerTestsFixture : EventHandlerTestsFixture<ChangedAccountNameEvent, ChangedAccountNameEventHandler>
    {
    }
}