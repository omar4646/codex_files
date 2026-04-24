using MediBookDesktop.Data;
using MediBookDesktop.Models;
using Microsoft.EntityFrameworkCore;

namespace MediBookDesktop.Services;

public class DoctorService
{
    private readonly AppDbContextFactory _contextFactory;

    public DoctorService(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Doctor>> SearchDoctorsAsync(string? name, string? specialty, string? location, DateTime? availableDate)
    {
        await using var db = _contextFactory.CreateDbContext();
        var query = db.Doctors.AsNoTracking().Where(doctor => doctor.IsActive);

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(doctor => doctor.FullName.ToLower().Contains(name.Trim().ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(specialty) && specialty != "All")
        {
            query = query.Where(doctor => doctor.Specialty == specialty);
        }

        if (!string.IsNullOrWhiteSpace(location) && location != "All")
        {
            query = query.Where(doctor => doctor.Location == location);
        }

        if (availableDate.HasValue)
        {
            var start = availableDate.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(doctor => doctor.AvailabilitySlots.Any(slot =>
                slot.IsAvailable && !slot.IsBooked && slot.StartTime >= start && slot.StartTime < end && slot.StartTime > DateTime.Now));
        }

        return await query.OrderBy(doctor => doctor.FullName).ToListAsync();
    }

    public async Task<List<Doctor>> GetAllDoctorsAsync(bool includeInactive = true)
    {
        await using var db = _contextFactory.CreateDbContext();
        var query = db.Doctors.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(doctor => doctor.IsActive);
        }

        return await query.OrderBy(doctor => doctor.FullName).ToListAsync();
    }

    public async Task<List<string>> GetSpecialtiesAsync()
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.Specialties.AsNoTracking().OrderBy(specialty => specialty.Name).Select(specialty => specialty.Name).ToListAsync();
    }

    public async Task<List<string>> GetLocationsAsync()
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.Doctors.AsNoTracking().Select(doctor => doctor.Location).Distinct().OrderBy(location => location).ToListAsync();
    }

    public async Task<(bool Success, string Message)> SaveDoctorAsync(Doctor doctor)
    {
        if (string.IsNullOrWhiteSpace(doctor.FullName) || string.IsNullOrWhiteSpace(doctor.Specialty) || string.IsNullOrWhiteSpace(doctor.Location))
        {
            return (false, "Name, specialty, and location are required.");
        }

        if (doctor.ConsultationFee <= 0)
        {
            return (false, "Consultation fee must be positive.");
        }

        await using var db = _contextFactory.CreateDbContext();
        if (doctor.Id == 0)
        {
            await db.Doctors.AddAsync(doctor);
        }
        else
        {
            var existing = await db.Doctors.FindAsync(doctor.Id);
            if (existing is null)
            {
                return (false, "Doctor not found.");
            }

            existing.FullName = doctor.FullName.Trim();
            existing.Specialty = doctor.Specialty.Trim();
            existing.Location = doctor.Location.Trim();
            existing.ConsultationFee = doctor.ConsultationFee;
            existing.Bio = doctor.Bio.Trim();
            existing.IsActive = doctor.IsActive;
        }

        await db.SaveChangesAsync();
        return (true, "Doctor saved.");
    }

    public async Task<(bool Success, string Message)> DeleteDoctorAsync(int doctorId)
    {
        await using var db = _contextFactory.CreateDbContext();
        var doctor = await db.Doctors.FindAsync(doctorId);
        if (doctor is null)
        {
            return (false, "Doctor not found.");
        }

        var hasFutureAppointments = await db.Appointments.AnyAsync(appointment =>
            appointment.DoctorId == doctorId &&
            appointment.AppointmentDateTime > DateTime.Now &&
            appointment.Status == AppointmentStatus.Booked);

        if (hasFutureAppointments)
        {
            return (false, "This doctor has future appointments and cannot be deleted.");
        }

        var hasHistoricalAppointments = await db.Appointments.AnyAsync(appointment => appointment.DoctorId == doctorId);
        if (hasHistoricalAppointments)
        {
            doctor.IsActive = false;
            await db.SaveChangesAsync();
            return (true, "Doctor has appointment history, so they were marked inactive instead of being permanently deleted.");
        }

        db.Doctors.Remove(doctor);
        await db.SaveChangesAsync();
        return (true, "Doctor deleted.");
    }
}
