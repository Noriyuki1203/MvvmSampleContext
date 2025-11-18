using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MvvmSampleContext.Models;

namespace MvvmSampleContext.ViewModels;

public partial class EditDepartmentViewModel : ObservableObject
{
    private readonly DepartmentRecord _original;
    private readonly IMessenger _messenger;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public EditDepartmentViewModel(DepartmentRecord record, IMessenger? messenger = null)
    {
        _messenger = messenger ?? WeakReferenceMessenger.Default;
        _original = new DepartmentRecord
        {
            Id = record.Id,
            Name = record.Name,
            UpdatedAt = record.UpdatedAt,
        };
        Name = _original.Name;
    }

    public string Title => _original.Id == 0 ? "部署の追加" : $"部署の編集 (ID: {_original.Id})";

    public DepartmentRecord? UpdatedRecord { get; private set; }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "部署名は必須です。";
            return;
        }

        ErrorMessage = string.Empty;

        var updated = new DepartmentRecord
        {
            Id = _original.Id,
            Name = Name.Trim(),
            UpdatedAt = _original.UpdatedAt,
        };

        UpdatedRecord = updated;
        _messenger.Send(new DialogCloseRequestedMessage(new DialogCloseRequestedEventArgs(true, updated)));
    }

    [RelayCommand]
    private void Cancel()
    {
        UpdatedRecord = null;
        _messenger.Send(new DialogCloseRequestedMessage(new DialogCloseRequestedEventArgs(false, null)));
    }
}
