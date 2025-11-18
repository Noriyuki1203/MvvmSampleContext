using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MvvmSampleContext.Models;

namespace MvvmSampleContext.ViewModels;

public partial class EditFamilyViewModel : ObservableObject
{
    private readonly FamilyMemberRecord _original;
    private readonly IMessenger _messenger;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string relationship = string.Empty;

    [ObservableProperty]
    private string ageText = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public EditFamilyViewModel(FamilyMemberRecord record, IMessenger? messenger = null)
    {
        _messenger = messenger ?? WeakReferenceMessenger.Default;
        _original = new FamilyMemberRecord
        {
            Id = record.Id,
            EmployeeId = record.EmployeeId,
            Name = record.Name,
            Relationship = record.Relationship,
            Age = record.Age,
            UpdatedAt = record.UpdatedAt,
        };

        Name = _original.Name;
        Relationship = _original.Relationship;
        AgeText = _original.Age > 0 ? _original.Age.ToString() : string.Empty;
    }

    public string Title => _original.Id == 0 ? "家族の追加" : $"家族の編集 (ID: {_original.Id})";

    public FamilyMemberRecord? UpdatedRecord { get; private set; }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Relationship))
        {
            ErrorMessage = "名前と続柄は必須です。";
            return;
        }

        if (!int.TryParse(AgeText, out var age) || age < 0)
        {
            ErrorMessage = "年齢は0以上の整数で入力してください。";
            return;
        }

        ErrorMessage = string.Empty;

        var updated = new FamilyMemberRecord
        {
            Id = _original.Id,
            EmployeeId = _original.EmployeeId,
            Name = Name.Trim(),
            Relationship = Relationship.Trim(),
            Age = age,
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
