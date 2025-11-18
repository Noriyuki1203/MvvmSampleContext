using System;

namespace MvvmSampleContext.Models;

public class FamilyMemberRecord
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Relationship { get; set; } = string.Empty;

    public int Age { get; set; }

    public DateTime UpdatedAt { get; set; }

    public override string ToString()
    {
        return $"{Name} ({Relationship})";
    }
}
