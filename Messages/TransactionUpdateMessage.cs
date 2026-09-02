using CommunityToolkit.Mvvm.Messaging.Messages;

namespace trackr.Messages
{
    public class TransactionUpdatedMessage : ValueChangedMessage<int>
    {
        public TransactionUpdatedMessage(int transactionId)
            : base(transactionId)
        {
        }
    }
}