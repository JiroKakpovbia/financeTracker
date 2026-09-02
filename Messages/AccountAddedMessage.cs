using CommunityToolkit.Mvvm.Messaging.Messages;

namespace trackr.Messages
{
    public sealed class AccountAddedMessage(Guid accountId) : ValueChangedMessage<Guid>(accountId)
    {
    }
}