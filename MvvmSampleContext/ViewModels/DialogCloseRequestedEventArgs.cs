using System;
using MvvmSampleContext.Models;

namespace MvvmSampleContext.ViewModels;

public class DialogCloseRequestedEventArgs : EventArgs
{
    public DialogCloseRequestedEventArgs(bool dialogResult, object? updatedRecord)
    {
        DialogResult = dialogResult;
        UpdatedRecord = updatedRecord;
    }

    public bool DialogResult { get; }

    public object? UpdatedRecord { get; }
}
