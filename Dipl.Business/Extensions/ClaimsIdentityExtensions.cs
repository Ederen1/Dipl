using System.Security.Claims;
using Dipl.Business.Entities;

namespace Dipl.Business.Extensions;

/// <summary>
///     Extension methods for <see cref="ClaimsIdentity" />.
/// </summary>
public static class ClaimsIdentityExtensions
{
    /// <summary>
    ///     Maps a <see cref="ClaimsIdentity" /> to a <see cref="User" /> object.
    /// </summary>
    /// <param name="identity">The claims identity to map.</param>
    /// <returns>A new <see cref="User" /> object populated from the identity claims.</returns>
    /// <exception cref="Exception">
    ///     Thrown if the user is not authenticated or if essential claims (Email, NameIdentifier) are
    ///     missing.
    /// </exception>
    public static User MapToUser(this ClaimsIdentity identity)
    {
        if (!identity.IsAuthenticated)
            throw new Exception("User is not authenticated");

        return new User
        {
            Email = GetEmail(identity),
            UserId = GetUserId(identity),
            UserName = identity.Name!
        };
    }

    /// <summary>
    ///     Retrieves the email claim from the identity.
    /// </summary>
    private static string GetEmail(ClaimsIdentity identity)
    {
        var email = identity.FindFirst(ClaimTypes.Email) ?? throw new Exception("Email claim not found");

        return email.Value;
    }

    /// <summary>
    ///     Retrieves the NameIdentifier (UserId) claim from the identity.
    /// </summary>
    private static string GetUserId(ClaimsIdentity identity)
    {
        var userId = identity.FindFirst(ClaimTypes.NameIdentifier) ??
                     throw new Exception("NameIdentifier claim not found");

        return userId.Value;
    }
}