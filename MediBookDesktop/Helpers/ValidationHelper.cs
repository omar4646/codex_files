using System.Text.RegularExpressions;

namespace MediBookDesktop.Helpers;

public static partial class ValidationHelper
{
    public static bool IsValidEmail(string email) => EmailRegex().IsMatch(email.Trim());

    public static string? Required(string value, string fieldName)
    {
        return string.IsNullOrWhiteSpace(value) ? $"{fieldName} is required." : null;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();
}
