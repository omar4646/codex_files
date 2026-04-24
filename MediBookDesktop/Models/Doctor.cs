using System.ComponentModel.DataAnnotations;

namespace MediBookDesktop.Models;

public class Doctor
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Specialty { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Location { get; set; } = string.Empty;

    public decimal ConsultationFee { get; set; }

    [MaxLength(1000)]
    public string Bio { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
