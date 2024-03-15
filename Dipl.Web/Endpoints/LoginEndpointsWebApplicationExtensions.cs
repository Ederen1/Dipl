using System.Security.Claims;
using Dipl.Business.Extensions;
using Dipl.Business.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;

namespace Dipl.Web.Endpoints;

public static class LoginEndpointsWebApplicationExtensions
{
    public static void MapLoginEndpoints(this WebApplication app)
    {
        app.MapGet(
            "/Account/Login",
            (string returnUrl) =>
            {
                var props = new AuthenticationProperties
                {
                    RedirectUri = "/Account/LoginCallback?ReturnUrl=" + returnUrl
                };

                return Results.Challenge(
                    props,
                    new[] { MicrosoftAccountDefaults.AuthenticationScheme }
                );
            }
        );

        app.MapGet(
            "/Account/LoginCallback",
            async (HttpContext context, UsersService userService, string returnUrl) =>
            {
                var identity = context.User.Identity as ClaimsIdentity;
                if (identity == null || !identity.IsAuthenticated)
                    return Results.Redirect("/");

                var user = identity.MapToUser();
                await userService.CreateIfNotExists(user);

                return Results.Redirect(returnUrl);
            }
        );

        app.MapGet(
            "/Account/Logout",
            async (HttpContext httpContext) =>
            {
                await httpContext.SignOutAsync();
                return Results.Redirect("/");
            }
        );
    }
}
