using System;
using MvvmSampleContext.Models;

namespace MvvmSampleContext.ViewModels;

public class DialogCloseRequestedEventArgs : EventArgs
{
    public DialogCloseRequestedEventArgs(bool dialogResult, EmployeeRecord? updatedRecord)
    {
        DialogResult = dialogResult;
        UpdatedRecord = updatedRecord;
    }

    public bool DialogResult { get; }

    public EmployeeRecord? UpdatedRecord { get; }
}
