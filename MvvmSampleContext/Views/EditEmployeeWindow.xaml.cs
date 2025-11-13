using System;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using MvvmSampleContext.ViewModels;

namespace MvvmSampleContext.Views;

public partial class EditEmployeeWindow : Window
{
    public EditEmployeeWindow()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<EditEmployeeWindow, DialogCloseRequestedMessage>(this, static (recipient, message) =>
        {
            recipient.OnCloseRequested(message.Value);
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        WeakReferenceMessenger.Default.Unregister<DialogCloseRequestedMessage>(this);
        base.OnClosed(e);
    }

    private void OnCloseRequested(DialogCloseRequestedEventArgs args)
    {
        DialogResult = args.DialogResult;
        Close();
    }
}
