using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmSampleContext.Models;

namespace MvvmSampleContext.ViewModels;

public partial class EditDroneViewModel : ObservableObject
{
    private readonly DroneRecord _original;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string serialNumber = string.Empty;

    [ObservableProperty]
    private string manufacturer = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public EditDroneViewModel(DroneRecord record)
    {
        _original = (DroneRecord)record.Clone();
        Name = _original.Name;
        SerialNumber = _original.SerialNumber;
        Manufacturer = _original.Manufacturer;
    }

    public string Title => _original.Id == 0 ? "ドローンの追加" : $"ドローンの編集 (ID: {_original.Id})";

    public event EventHandler<DialogCloseRequestedEventArgs>? CloseRequested;

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(SerialNumber))
        {
            ErrorMessage = "名前とシリアル番号は必須です。";
            return;
        }

        ErrorMessage = string.Empty;

        var updated = new DroneRecord
        {
            Id = _original.Id,
            Name = Name.Trim(),
            SerialNumber = SerialNumber.Trim(),
            Manufacturer = Manufacturer.Trim(),
        };

        CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(true, updated));
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(false, null));
    }
}
