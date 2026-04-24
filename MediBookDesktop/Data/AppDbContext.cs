using System.IO;
using MediBookDesktop.Models;
using Microsoft.EntityFrameworkCore;

namespace MediBookDesktop.Data;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Specialty> Specialties => Set<Specialty>();

    public static string DefaultDatabasePath
    {
        get
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MediBook Desktop");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "medibook.db");
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite($"Data Source={DefaultDatabasePath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<Doctor>()
            .Property(doctor => doctor.ConsultationFee)
            .HasConversion<double>();

        modelBuilder.Entity<AvailabilitySlot>()
            .HasIndex(slot => new { slot.DoctorId, slot.StartTime })
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasOne(appointment => appointment.Patient)
            .WithMany(user => user.Appointments)
            .HasForeignKey(appointment => appointment.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(appointment => appointment.Doctor)
            .WithMany(doctor => doctor.Appointments)
            .HasForeignKey(appointment => appointment.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(appointment => appointment.AvailabilitySlot)
            .WithMany()
            .HasForeignKey(appointment => appointment.AvailabilitySlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
