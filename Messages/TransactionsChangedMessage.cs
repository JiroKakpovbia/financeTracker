using CommunityToolkit.Mvvm.Messaging.Messages;

namespace trackr.Messages
{
    public class TransactionsChangedMessage
        : ValueChangedMessage<Guid?>
    {
        public TransactionsChangedMessage(Guid? accountId = null)
            : base(accountId)
        {
        }
    }
}