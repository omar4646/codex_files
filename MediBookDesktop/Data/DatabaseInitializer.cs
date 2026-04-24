using MediBookDesktop.Helpers;
using MediBookDesktop.Models;
using Microsoft.EntityFrameworkCore;

namespace MediBookDesktop.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(AppDbContextFactory contextFactory)
    {
        await using var db = contextFactory.CreateDbContext();
        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync())
        {
            await EnsureSlotsAsync(db);
            return;
        }

        var users = new List<User>
        {
            new()
            {
                FullName = "Clinic Administrator",
                Email = "admin@medibook.local",
                PasswordHash = PasswordHasher.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                FullName = "Maya Chen",
                Email = "maya@example.local",
                PasswordHash = PasswordHasher.HashPassword("Patient123!"),
                Role = UserRole.Patient,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                FullName = "Noah Patel",
                Email = "noah@example.local",
                PasswordHash = PasswordHasher.HashPassword("Patient123!"),
                Role = UserRole.Patient,
                CreatedAt = DateTime.UtcNow
            }
        };

        var specialties = new[]
        {
            "Cardiology", "Dermatology", "Family Medicine", "Neurology", "Pediatrics", "Orthopedics"
        }.Select(name => new Specialty { Name = name }).ToList();

        var doctors = new List<Doctor>
        {
            new() { FullName = "Dr. Elena Brooks", Specialty = "Cardiology", Location = "Downtown Clinic", ConsultationFee = 135, Bio = "Heart health specialist focused on preventive care and long-term cardiac wellness." },
            new() { FullName = "Dr. Aaron Kim", Specialty = "Dermatology", Location = "Northside Medical Center", ConsultationFee = 110, Bio = "Treats adult and adolescent skin concerns with practical, evidence-based care." },
            new() { FullName = "Dr. Priya Raman", Specialty = "Family Medicine", Location = "Riverside Health Hub", ConsultationFee = 85, Bio = "Primary care doctor for routine checkups, chronic care, and everyday health needs." },
            new() { FullName = "Dr. Mateo Silva", Specialty = "Neurology", Location = "Downtown Clinic", ConsultationFee = 160, Bio = "Neurology consultant for headaches, neuropathy, tremor, and seizure follow-up." },
            new() { FullName = "Dr. Grace Turner", Specialty = "Pediatrics", Location = "Northside Medical Center", ConsultationFee = 95, Bio = "Pediatrician helping children and families with compassionate, clear care plans." },
            new() { FullName = "Dr. Hassan Ali", Specialty = "Orthopedics", Location = "Riverside Health Hub", ConsultationFee = 140, Bio = "Orthopedic care for sports injuries, joint pain, and post-operative follow-up." },
            new() { FullName = "Dr. Sophie Laurent", Specialty = "Family Medicine", Location = "Downtown Clinic", ConsultationFee = 90, Bio = "Family doctor with a focus on lifestyle medicine and preventive screening." },
            new() { FullName = "Dr. Victor Nguyen", Specialty = "Dermatology", Location = "Riverside Health Hub", ConsultationFee = 120, Bio = "Dermatology specialist for acne, eczema, mole checks, and minor procedures." }
        };

        await db.Users.AddRangeAsync(users);
        await db.Specialties.AddRangeAsync(specialties);
        await db.Doctors.AddRangeAsync(doctors);
        await db.SaveChangesAsync();

        await EnsureSlotsAsync(db);
    }

    private static async Task EnsureSlotsAsync(AppDbContext db)
    {
        var doctors = await db.Doctors.ToListAsync();
        var existingKeys = await db.AvailabilitySlots
            .Select(slot => slot.DoctorId + "|" + slot.StartTime.ToString("O"))
            .ToListAsync();
        var existing = existingKeys.ToHashSet();

        var times = new[]
        {
            new TimeSpan(9, 0, 0),
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            new TimeSpan(14, 0, 0),
            new TimeSpan(15, 0, 0),
            new TimeSpan(16, 0, 0)
        };

        var slots = new List<AvailabilitySlot>();
        var startDate = DateTime.Today;

        foreach (var doctor in doctors)
        {
            for (var day = 0; day < 14; day++)
            {
                var date = startDate.AddDays(day);
                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    continue;
                }

                foreach (var time in times)
                {
                    var start = date.Date.Add(time);
                    var key = doctor.Id + "|" + start.ToString("O");
                    if (existing.Contains(key))
                    {
                        continue;
                    }

                    slots.Add(new AvailabilitySlot
                    {
                        DoctorId = doctor.Id,
                        StartTime = start,
                        EndTime = start.AddMinutes(45),
                        IsBooked = false,
                        IsAvailable = true
                    });
                }
            }
        }

        if (slots.Count > 0)
        {
            await db.AvailabilitySlots.AddRangeAsync(slots);
            await db.SaveChangesAsync();
        }
    }
}
