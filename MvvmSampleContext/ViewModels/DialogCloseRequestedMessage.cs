using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MvvmSampleContext.ViewModels;

public sealed class DialogCloseRequestedMessage : ValueChangedMessage<DialogCloseRequestedEventArgs>
{
    public DialogCloseRequestedMessage(DialogCloseRequestedEventArgs value)
        : base(value)
    {
    }
}
