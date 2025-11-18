using MvvmSampleContext.Models;

namespace MvvmSampleContext.Services;

public interface IEditDepartmentDialogService
{
    DepartmentRecord? ShowInputDialog(DepartmentRecord record);
}
