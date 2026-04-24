namespace MediBookDesktop.Models;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public User? Patient { get; set; }
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public int AvailabilitySlotId { get; set; }
    public AvailabilitySlot? AvailabilitySlot { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string ReasonForVisit { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }

    public bool IsUpcoming => AppointmentDateTime > DateTime.Now && Status == AppointmentStatus.Booked;
    public string AppointmentCode => $"APT-{Id:00000}";
}
