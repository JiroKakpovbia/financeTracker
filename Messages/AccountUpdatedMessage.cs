using CommunityToolkit.Mvvm.Messaging.Messages;

namespace trackr.Messages
{
    public sealed class AccountUpdatedMessage(Guid accountId) : ValueChangedMessage<Guid>(accountId)
    {
    }
}