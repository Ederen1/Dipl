using System.Security.Claims;
using Dipl.Business.Entities;
using Dipl.Business.Extensions;
using Microsoft.AspNetCore.Components.Authorization;

namespace Dipl.Business.Services;

/// <summary>
///     Service for retrieving information about the currently authenticated user.
/// </summary>
public class UserAuthenticationService(AuthenticationStateProvider authenticationStateProvider)
{
    /// <summary>
    ///     Gets the <see cref="User" /> object for the currently authenticated user.
    /// </summary>
    /// <returns>A <see cref="User" /> object if the user is authenticated; otherwise, null.</returns>
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