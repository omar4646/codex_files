using MediBookDesktop.Data;
using MediBookDesktop.Models;
using Microsoft.EntityFrameworkCore;

namespace MediBookDesktop.Services;

public class AppointmentService
{
    private readonly AppDbContextFactory _contextFactory;

    public AppointmentService(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<(bool Success, string Message, Appointment? Appointment)> BookAppointmentAsync(int patientId, int doctorId, int slotId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return (false, "Reason for visit is required.", null);
        }

        await using var db = _contextFactory.CreateDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var slot = await db.AvailabilitySlots.FirstOrDefaultAsync(item => item.Id == slotId && item.DoctorId == doctorId);
        if (slot is null || !slot.IsAvailable)
        {
            return (false, "Selected time slot is no longer available.", null);
        }

        if (slot.StartTime <= DateTime.Now)
        {
            return (false, "Appointments cannot be booked in the past.", null);
        }

        if (slot.IsBooked)
        {
            return (false, "Selected time slot is already booked.", null);
        }

        var duplicate = await db.Appointments.AnyAsync(appointment =>
            appointment.PatientId == patientId &&
            appointment.DoctorId == doctorId &&
            appointment.AvailabilitySlotId == slotId &&
            appointment.Status == AppointmentStatus.Booked);

        if (duplicate)
        {
            return (false, "You already have this appointment booked.", null);
        }

        slot.IsBooked = true;
        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            AvailabilitySlotId = slotId,
            AppointmentDateTime = slot.StartTime,
            ReasonForVisit = reason.Trim(),
            Status = AppointmentStatus.Booked,
            CreatedAt = DateTime.UtcNow
        };

        await db.Appointments.AddAsync(appointment);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        appointment.Doctor = await db.Doctors.FindAsync(doctorId);
        appointment.AvailabilitySlot = slot;
        return (true, "Appointment booked.", appointment);
    }

    public async Task<List<Appointment>> GetPatientAppointmentsAsync(int patientId)
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.Appointments.AsNoTracking()
            .Include(appointment => appointment.Doctor)
            .Include(appointment => appointment.AvailabilitySlot)
            .Where(appointment => appointment.PatientId == patientId)
            .OrderByDescending(appointment => appointment.AppointmentDateTime)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetAppointmentsAsync(int? doctorId, string? patientText, DateTime? date, AppointmentStatus? status)
    {
        await using var db = _contextFactory.CreateDbContext();
        var query = db.Appointments.AsNoTracking()
            .Include(appointment => appointment.Doctor)
            .Include(appointment => appointment.Patient)
            .Include(appointment => appointment.AvailabilitySlot)
            .AsQueryable();

        if (doctorId.HasValue && doctorId.Value > 0)
        {
            query = query.Where(appointment => appointment.DoctorId == doctorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(patientText))
        {
            var lowered = patientText.Trim().ToLower();
            query = query.Where(appointment => appointment.Patient != null &&
                (appointment.Patient.FullName.ToLower().Contains(lowered) || appointment.Patient.Email.ToLower().Contains(lowered)));
        }

        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(appointment => appointment.AppointmentDateTime >= start && appointment.AppointmentDateTime < end);
        }

        if (status.HasValue)
        {
            query = query.Where(appointment => appointment.Status == status.Value);
        }

        return await query.OrderByDescending(appointment => appointment.AppointmentDateTime).ToListAsync();
    }

    public async Task<(bool Success, string Message)> CancelAppointmentAsync(int appointmentId)
    {
        await using var db = _contextFactory.CreateDbContext();
        var appointment = await db.Appointments.Include(item => item.AvailabilitySlot).FirstOrDefaultAsync(item => item.Id == appointmentId);
        if (appointment is null)
        {
            return (false, "Appointment not found.");
        }

        if (appointment.Status != AppointmentStatus.Booked)
        {
            return (false, "Only booked appointments can be cancelled.");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledAt = DateTime.UtcNow;
        if (appointment.AvailabilitySlot is not null)
        {
            appointment.AvailabilitySlot.IsBooked = false;
        }

        await db.SaveChangesAsync();
        return (true, "Appointment cancelled and the slot is open again.");
    }

    public async Task<(bool Success, string Message)> RescheduleAppointmentAsync(int appointmentId, int newSlotId)
    {
        await using var db = _contextFactory.CreateDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var appointment = await db.Appointments.Include(item => item.AvailabilitySlot).FirstOrDefaultAsync(item => item.Id == appointmentId);
        var newSlot = await db.AvailabilitySlots.FindAsync(newSlotId);

        if (appointment is null || newSlot is null)
        {
            return (false, "Appointment or slot not found.");
        }

        if (appointment.Status != AppointmentStatus.Booked || appointment.AppointmentDateTime <= DateTime.Now)
        {
            return (false, "Only upcoming booked appointments can be rescheduled.");
        }

        if (!newSlot.IsAvailable || newSlot.IsBooked || newSlot.StartTime <= DateTime.Now)
        {
            return (false, "The selected new slot is not available.");
        }

        if (newSlot.DoctorId != appointment.DoctorId)
        {
            return (false, "Rescheduling keeps the same doctor. Choose a slot for the current doctor.");
        }

        if (appointment.AvailabilitySlot is not null)
        {
            appointment.AvailabilitySlot.IsBooked = false;
        }

        newSlot.IsBooked = true;
        appointment.AvailabilitySlotId = newSlot.Id;
        appointment.AppointmentDateTime = newSlot.StartTime;

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return (true, "Appointment rescheduled.");
    }

    public async Task<(bool Success, string Message)> UpdateStatusAsync(int appointmentId, AppointmentStatus status)
    {
        await using var db = _contextFactory.CreateDbContext();
        var appointment = await db.Appointments.Include(item => item.AvailabilitySlot).FirstOrDefaultAsync(item => item.Id == appointmentId);
        if (appointment is null)
        {
            return (false, "Appointment not found.");
        }

        appointment.Status = status;
        if (status == AppointmentStatus.Cancelled)
        {
            appointment.CancelledAt = DateTime.UtcNow;
            if (appointment.AvailabilitySlot is not null)
            {
                appointment.AvailabilitySlot.IsBooked = false;
            }
        }
        else if (appointment.AvailabilitySlot is not null && status == AppointmentStatus.Booked)
        {
            if (appointment.AvailabilitySlot.IsBooked && appointment.AppointmentDateTime > DateTime.Now)
            {
                var otherAppointmentUsesSlot = await db.Appointments.AnyAsync(item =>
                    item.Id != appointment.Id &&
                    item.AvailabilitySlotId == appointment.AvailabilitySlotId &&
                    item.Status == AppointmentStatus.Booked);
                if (otherAppointmentUsesSlot)
                {
                    return (false, "Another booked appointment already uses this slot.");
                }
            }

            appointment.AvailabilitySlot.IsBooked = true;
        }

        await db.SaveChangesAsync();
        return (true, "Appointment status updated.");
    }

    public async Task<(int TotalDoctors, int UpcomingAppointments, int TodayAppointments, int CancelledAppointments)> GetAdminStatsAsync()
    {
        await using var db = _contextFactory.CreateDbContext();
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        return (
            await db.Doctors.CountAsync(),
            await db.Appointments.CountAsync(item => item.AppointmentDateTime > DateTime.Now && item.Status == AppointmentStatus.Booked),
            await db.Appointments.CountAsync(item => item.AppointmentDateTime >= today && item.AppointmentDateTime < tomorrow),
            await db.Appointments.CountAsync(item => item.Status == AppointmentStatus.Cancelled)
        );
    }
}
