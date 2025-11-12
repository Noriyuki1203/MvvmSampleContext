using MvvmSampleContext.Models;

namespace MvvmSampleContext.Services;

public interface IEditDroneDialogService
{
    DroneRecord? ShowEditDialog(DroneRecord record);
}
