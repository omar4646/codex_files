namespace MediBookDesktop.Models;

public class AvailabilitySlot
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsBooked { get; set; }
    public bool IsAvailable { get; set; } = true;

    public string DisplayTime => $"{StartTime:MMM d, yyyy h:mm tt} - {EndTime:h:mm tt}";
    public string ShortTime => $"{StartTime:h:mm tt} - {EndTime:h:mm tt}";
}
