using MediBookDesktop.Data;
using MediBookDesktop.Models;
using Microsoft.EntityFrameworkCore;

namespace MediBookDesktop.Services;

public class SpecialtyService
{
    private readonly AppDbContextFactory _contextFactory;

    public SpecialtyService(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Specialty>> GetSpecialtiesAsync()
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.Specialties.AsNoTracking().OrderBy(item => item.Name).ToListAsync();
    }

    public async Task<(bool Success, string Message)> SaveAsync(Specialty specialty)
    {
        if (string.IsNullOrWhiteSpace(specialty.Name))
        {
            return (false, "Specialty name is required.");
        }

        await using var db = _contextFactory.CreateDbContext();
        var normalized = specialty.Name.Trim();
        var duplicate = await db.Specialties.AnyAsync(item => item.Id != specialty.Id && item.Name.ToLower() == normalized.ToLower());
        if (duplicate)
        {
            return (false, "That specialty already exists.");
        }

        if (specialty.Id == 0)
        {
            await db.Specialties.AddAsync(new Specialty { Name = normalized });
        }
        else
        {
            var existing = await db.Specialties.FindAsync(specialty.Id);
            if (existing is null)
            {
                return (false, "Specialty not found.");
            }

            existing.Name = normalized;
        }

        await db.SaveChangesAsync();
        return (true, "Specialty saved.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int specialtyId)
    {
        await using var db = _contextFactory.CreateDbContext();
        var specialty = await db.Specialties.FindAsync(specialtyId);
        if (specialty is null)
        {
            return (false, "Specialty not found.");
        }

        var inUse = await db.Doctors.AnyAsync(doctor => doctor.Specialty == specialty.Name);
        if (inUse)
        {
            return (false, "Specialties assigned to doctors cannot be deleted.");
        }

        db.Specialties.Remove(specialty);
        await db.SaveChangesAsync();
        return (true, "Specialty deleted.");
    }
}
