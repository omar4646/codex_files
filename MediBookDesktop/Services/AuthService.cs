using MediBookDesktop.Data;
using MediBookDesktop.Helpers;
using MediBookDesktop.Models;
using Microsoft.EntityFrameworkCore;

namespace MediBookDesktop.Services;

public class AuthService
{
    private readonly AppDbContextFactory _contextFactory;

    public AuthService(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public User? CurrentUser { get; private set; }

    public async Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password)
    {
        await using var db = _contextFactory.CreateDbContext();
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(item => item.Email.ToLower() == normalizedEmail);

        if (user is null || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return (false, "Invalid email or password.", null);
        }

        CurrentUser = user;
        return (true, $"Welcome back, {user.FullName}.", user);
    }

    public async Task<(bool Success, string Message, User? User)> RegisterPatientAsync(string fullName, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return (false, "Full name is required.", null);
        }

        if (!ValidationHelper.IsValidEmail(email))
        {
            return (false, "Enter a valid email address.", null);
        }

        if (password.Length < 8)
        {
            return (false, "Password must be at least 8 characters.", null);
        }

        await using var db = _contextFactory.CreateDbContext();
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var exists = await db.Users.AnyAsync(item => item.Email.ToLower() == normalizedEmail);
        if (exists)
        {
            return (false, "An account already exists with that email.", null);
        }

        var user = new User
        {
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = PasswordHasher.HashPassword(password),
            Role = UserRole.Patient,
            CreatedAt = DateTime.UtcNow
        };

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
        CurrentUser = user;
        return (true, "Registration complete.", user);
    }

    public void Logout()
    {
        CurrentUser = null;
    }
}
