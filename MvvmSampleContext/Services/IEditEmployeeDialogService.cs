using MvvmSampleContext.Models;

namespace MvvmSampleContext.Services;

public interface IEditEmployeeDialogService
{
    EmployeeRecord? ShowEditDialog(EmployeeRecord record);
}
