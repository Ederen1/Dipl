using System.Security.Claims;
using Dipl.Business.Entities;
using Dipl.Business.Extensions;
using Microsoft.AspNetCore.Components.Authorization;

namespace Dipl.Business.Services;

public class UserAuthenticationService(AuthenticationStateProvider authenticationStateProvider)
{
    public async Task<User?> GetUserInfo()
    {
        var identity = (await authenticationStateProvider.GetAuthenticationStateAsync()).User.Identity;

        if (identity is not ClaimsIdentity claimsIdentity || claimsIdentity.IsAuthenticated == false)
            return null;

        return claimsIdentity.MapToUser();
    }

    public async Task<bool> IsLoggedIn()
    {
        return await GetUserInfo() is not null;
    }
}