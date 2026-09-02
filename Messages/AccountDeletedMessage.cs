using CommunityToolkit.Mvvm.Messaging.Messages;

namespace trackr.Messages
{
    public sealed class AccountDeletedMessage(Guid accountId) : ValueChangedMessage<Guid>(accountId)
    {
    }
}