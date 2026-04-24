using MediBookDesktop.Data;
using MediBookDesktop.Helpers;
using MediBookDesktop.Models;
using MediBookDesktop.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MediBookDesktop.Tests;

public class AppointmentServiceTests
{
    [Fact]
    public async Task AppointmentCannotBeDoubleBooked()
    {
        var factory = await CreateSeededFactoryAsync();
        var service = new AppointmentService(factory);

        var first = await service.BookAppointmentAsync(1, 1, 1, "Annual check");
        var second = await service.BookAppointmentAsync(2, 1, 1, "Second booking");

        Assert.True(first.Success);
        Assert.False(second.Success);
    }

    [Fact]
    public async Task CancelledAppointmentFreesSlot()
    {
        var factory = await CreateSeededFactoryAsync();
        var service = new AppointmentService(factory);

        var booking = await service.BookAppointmentAsync(1, 1, 1, "Annual check");
        Assert.True(booking.Success);

        var cancelled = await service.CancelAppointmentAsync(booking.Appointment!.Id);
        Assert.True(cancelled.Success);

        await using var db = factory.CreateDbContext();
        var slot = await db.AvailabilitySlots.FindAsync(1);
        Assert.False(slot!.IsBooked);
    }

    [Fact]
    public async Task PatientCannotBookPastSlot()
    {
        var factory = await CreateSeededFactoryAsync(includePastSlot: true);
        var service = new AppointmentService(factory);

        var result = await service.BookAppointmentAsync(1, 1, 2, "Past visit");

        Assert.False(result.Success);
        Assert.Contains("past", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PasswordHashingVerifiesCorrectPasswordOnly()
    {
        var hash = PasswordHasher.HashPassword("Secret123!");

        Assert.True(PasswordHasher.VerifyPassword("Secret123!", hash));
        Assert.False(PasswordHasher.VerifyPassword("wrong-password", hash));
        Assert.DoesNotContain("Secret123!", hash);
    }

    [Fact]
    public async Task AdminCanAddDoctor()
    {
        var factory = await CreateSeededFactoryAsync();
        var service = new DoctorService(factory);

        var result = await service.SaveDoctorAsync(new Doctor
        {
            FullName = "Dr. Test Admin",
            Specialty = "Cardiology",
            Location = "Downtown Clinic",
            ConsultationFee = 125,
            Bio = "Test doctor",
            IsActive = true
        });

        Assert.True(result.Success);
        Assert.Contains((await service.GetAllDoctorsAsync()).Select(doctor => doctor.FullName), name => name == "Dr. Test Admin");
    }

    private static async Task<AppDbContextFactory> CreateSeededFactoryAsync(bool includePastSlot = false)
    {
        var path = Path.Combine(Path.GetTempPath(), $"medibook-test-{Guid.NewGuid():N}.db");
        var factory = new AppDbContextFactory(path);
        await using var db = factory.CreateDbContext();
        await db.Database.EnsureCreatedAsync();

        await db.Users.AddRangeAsync(
            new User { Id = 1, FullName = "Patient One", Email = "one@example.local", PasswordHash = PasswordHasher.HashPassword("Patient123!"), Role = UserRole.Patient },
            new User { Id = 2, FullName = "Patient Two", Email = "two@example.local", PasswordHash = PasswordHasher.HashPassword("Patient123!"), Role = UserRole.Patient });

        await db.Doctors.AddAsync(new Doctor
        {
            Id = 1,
            FullName = "Dr. Unit Test",
            Specialty = "Cardiology",
            Location = "Downtown Clinic",
            ConsultationFee = 100,
            Bio = "Test fixture",
            IsActive = true
        });

        await db.Specialties.AddAsync(new Specialty { Name = "Cardiology" });
        await db.AvailabilitySlots.AddAsync(new AvailabilitySlot
        {
            Id = 1,
            DoctorId = 1,
            StartTime = DateTime.Now.AddDays(1).Date.AddHours(10),
            EndTime = DateTime.Now.AddDays(1).Date.AddHours(10).AddMinutes(45),
            IsAvailable = true,
            IsBooked = false
        });

        if (includePastSlot)
        {
            await db.AvailabilitySlots.AddAsync(new AvailabilitySlot
            {
                Id = 2,
                DoctorId = 1,
                StartTime = DateTime.Now.AddDays(-1),
                EndTime = DateTime.Now.AddDays(-1).AddMinutes(45),
                IsAvailable = true,
                IsBooked = false
            });
        }

        await db.SaveChangesAsync();
        return factory;
    }
}
