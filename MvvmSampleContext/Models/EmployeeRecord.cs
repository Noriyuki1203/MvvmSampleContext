using System;

namespace MvvmSampleContext.Models;

public class EmployeeRecord : ICloneable
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string EmployeeNumber { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }

    public override string ToString()
    {
        return $"{Name} ({EmployeeNumber})";
    }
}
