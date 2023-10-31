using System.Security.Claims;
using Dipl.Common.Types;

namespace Dipl.Common.Extensions;

public static class ClaimsIdentityExtensions
{
    public static UserInfo MapToUserInfo(this ClaimsIdentity identity)
    {
        return new UserInfo
        {
            UserId = GetUserId(identity),
            Email = GetEmail(identity)
        };
    }

    private static string GetEmail(ClaimsIdentity identity)
    {
        var email = identity.FindFirst(ClaimTypes.Email) ?? throw new Exception("Email claim not found");

        return email.Value;
    }

    private static Guid GetUserId(ClaimsIdentity identity)
    {
        var userId = identity.FindFirst(ClaimTypes.NameIdentifier) ?? throw new Exception("NameIdentifier claim not found");

        return Guid.Parse(userId.Value);
    }
}