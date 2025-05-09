using System.Net.Mail;

namespace Dipl.Common.Util;

public static class StringExtensions
{
    /// <summary>
    ///     Attempts to parse a string as an email address.
    /// </summary>
    /// <param name="email">The string to parse.</param>
    /// <returns>The trimmed email address if valid; otherwise, null.</returns>
    public static string? ParseAsEmail(this string email)
    {
        var trimmed = email.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return null;

        try
        {
            var addr = new MailAddress(trimmed);
            return addr.Address == trimmed ? trimmed : null;
        }
        catch
        {
            return null;
        }
    }
}