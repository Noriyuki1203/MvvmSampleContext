using System;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using MvvmSampleContext.ViewModels;

namespace MvvmSampleContext.Views;

/// <summary>
/// Base window that listens for <see cref="DialogCloseRequestedMessage"/> and closes itself.
/// </summary>
public abstract class DialogWindowBase : Window
{
    private readonly IMessenger _messenger;

    protected DialogWindowBase()
        : this(null)
    {
    }

    protected DialogWindowBase(IMessenger? messenger)
    {
        _messenger = messenger ?? WeakReferenceMessenger.Default;
        Loaded += OnDialogLoaded;
    }

    private void OnDialogLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnDialogLoaded;
        _messenger.Register<DialogWindowBase, DialogCloseRequestedMessage>(this, static (recipient, message) =>
        {
            recipient.HandleCloseRequested(message.Value);
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        _messenger.Unregister<DialogCloseRequestedMessage>(this);
        base.OnClosed(e);
    }

    /// <summary>
    /// Default close behavior sets <see cref="Window.DialogResult"/> and closes the window.
    /// Override for custom handling before the window is closed.
    /// </summary>
    protected virtual void HandleCloseRequested(DialogCloseRequestedEventArgs args)
    {
        DialogResult = args.DialogResult;
        Close();
    }
}
