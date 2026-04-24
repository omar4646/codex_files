using System.ComponentModel.DataAnnotations;

namespace MediBookDesktop.Models;

public class Specialty
{
    public int Id { get; set; }

    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;
}
