using CommunityToolkit.Mvvm.Messaging.Messages;

namespace trackr.Messages
{
    public sealed class TransactionUpdatedMessage(int transactionId) : ValueChangedMessage<int>(transactionId)
    {
    }
}