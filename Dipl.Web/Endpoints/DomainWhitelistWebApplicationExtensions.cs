namespace Dipl.Web.Endpoints;

public static class DomainWhitelistWebApplicationExtensions
{
    /**
     * This middleware checks if the users email is in the allowed domains list.
     */
    public static void UseDomainWhitelist(this WebApplication app)
    {
        // var configuration = app.Services.GetService<IConfiguration>()!;
        // app.Use(
        //     async (context, next) =>
        //     {
        //         if (context.User.Identity?.IsAuthenticated != true)
        //         {
        //             await next(context);
        //             return;
        //         }
        //
        //         var email = context.User.FindFirst(ClaimTypes.Email)?.Value;
        //         var allowedDomains = configuration.GetSection("AllowedDomains").Get<string[]>()!;
        //
        //         if (email is null || !allowedDomains.Any(x => email.EndsWith(x)))
        //             await context.SignOutAsync();
        //
        //         await next(context);
        //     }
        // );
    }
}