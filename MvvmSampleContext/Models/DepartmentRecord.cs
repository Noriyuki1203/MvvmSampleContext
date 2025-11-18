using System;

namespace MvvmSampleContext.Models;

public class DepartmentRecord
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; }

    public override string ToString()
    {
        return Name;
    }
}
