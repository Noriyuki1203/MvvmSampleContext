using MvvmSampleContext.Models;

namespace MvvmSampleContext.Services;

public interface IEditFamilyDialogService
{
    FamilyMemberRecord? ShowInputDialog(FamilyMemberRecord record);
}
