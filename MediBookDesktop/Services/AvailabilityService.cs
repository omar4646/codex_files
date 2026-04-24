using MediBookDesktop.Data;
using MediBookDesktop.Models;
using Microsoft.EntityFrameworkCore;

namespace MediBookDesktop.Services;

public class AvailabilityService
{
    private readonly AppDbContextFactory _contextFactory;

    public AvailabilityService(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<AvailabilitySlot>> GetAvailableSlotsAsync(int doctorId, DateTime? date = null)
    {
        await using var db = _contextFactory.CreateDbContext();
        var query = db.AvailabilitySlots.AsNoTracking()
            .Where(slot => slot.DoctorId == doctorId && slot.IsAvailable && !slot.IsBooked && slot.StartTime > DateTime.Now);

        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(slot => slot.StartTime >= start && slot.StartTime < end);
        }

        return await query.OrderBy(slot => slot.StartTime).ToListAsync();
    }

    public async Task<List<AvailabilitySlot>> GetSlotsForDoctorAsync(int doctorId, DateTime? date = null)
    {
        await using var db = _contextFactory.CreateDbContext();
        var query = db.AvailabilitySlots.AsNoTracking().Where(slot => slot.DoctorId == doctorId);

        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(slot => slot.StartTime >= start && slot.StartTime < end);
        }

        return await query.OrderBy(slot => slot.StartTime).ToListAsync();
    }

    public async Task<(bool Success, string Message)> AddSlotAsync(int doctorId, DateTime startTime, DateTime endTime)
    {
        if (doctorId <= 0)
        {
            return (false, "Select a doctor.");
        }

        if (startTime <= DateTime.Now)
        {
            return (false, "Availability must be in the future.");
        }

        if (endTime <= startTime)
        {
            return (false, "End time must be after start time.");
        }

        await using var db = _contextFactory.CreateDbContext();
        var exists = await db.AvailabilitySlots.AnyAsync(slot => slot.DoctorId == doctorId && slot.StartTime == startTime);
        if (exists)
        {
            return (false, "A slot already exists at that time for this doctor.");
        }

        await db.AvailabilitySlots.AddAsync(new AvailabilitySlot
        {
            DoctorId = doctorId,
            StartTime = startTime,
            EndTime = endTime,
            IsAvailable = true,
            IsBooked = false
        });
        await db.SaveChangesAsync();
        return (true, "Availability added.");
    }

    public async Task<(bool Success, string Message)> MarkUnavailableAsync(int slotId)
    {
        await using var db = _contextFactory.CreateDbContext();
        var slot = await db.AvailabilitySlots.FindAsync(slotId);
        if (slot is null)
        {
            return (false, "Slot not found.");
        }

        if (slot.IsBooked)
        {
            return (false, "Booked slots cannot be marked unavailable. Cancel or reschedule the appointment first.");
        }

        slot.IsAvailable = false;
        await db.SaveChangesAsync();
        return (true, "Slot marked unavailable.");
    }
}
