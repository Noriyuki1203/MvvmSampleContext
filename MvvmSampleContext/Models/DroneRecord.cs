using System;

namespace MvvmSampleContext.Models;

public class DroneRecord : ICloneable
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }

    public override string ToString()
    {
        return Name;
    }
}
