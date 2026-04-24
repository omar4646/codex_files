using System.ComponentModel.DataAnnotations;

namespace MediBookDesktop.Models;

public class User
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(180)]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
