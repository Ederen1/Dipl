using System.Net.Mail;

namespace Dipl.Common.Util;

public static class StringExtensions
{
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