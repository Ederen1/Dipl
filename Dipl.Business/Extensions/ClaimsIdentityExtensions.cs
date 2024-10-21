using System.Security.Claims;
using Dipl.Business.Entities;

namespace Dipl.Business.Extensions;

public static class ClaimsIdentityExtensions
{
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

    private static string GetEmail(ClaimsIdentity identity)
    {
        var email =
            identity.FindFirst(ClaimTypes.Email) ?? throw new Exception("Email claim not found");

        return email.Value;
    }

    private static Guid GetUserId(ClaimsIdentity identity)
    {
        var userId =
            identity.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new Exception("NameIdentifier claim not found");

        return Guid.Parse(userId.Value);
    }
}