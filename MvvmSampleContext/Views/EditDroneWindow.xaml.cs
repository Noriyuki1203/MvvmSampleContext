using System.Windows;
using MvvmSampleContext.Models;
using MvvmSampleContext.ViewModels;

namespace MvvmSampleContext.Views;

public partial class EditDroneWindow : Window
{
    public EditDroneWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    public DroneRecord? Result { get; private set; }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is EditDroneViewModel oldVm)
        {
            oldVm.CloseRequested -= OnCloseRequested;
        }

        if (e.NewValue is EditDroneViewModel newVm)
        {
            newVm.CloseRequested += OnCloseRequested;
        }
    }

    private void OnCloseRequested(object? sender, DialogCloseRequestedEventArgs e)
    {
        Result = e.UpdatedRecord;
        DialogResult = e.DialogResult;
        Close();
    }
}
