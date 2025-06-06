using Dipl.Business.Entities;
using Microsoft.Extensions.Logging;

namespace Dipl.Business.Services;

public class UsersService(
    AppDbContext dbContext,
    UserAuthenticationService userAuthenticationService,
    ILogger<UsersService> _logger)
{
    public async Task CreateIfNotExists(User user)
    {
        if (await dbContext.Users.FindAsync(user.UserId) != null)
            return;

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Retrieves the currently authenticated user. If no user is authenticated, returns the guest user.
    ///     If an authenticated user is not found in the database, an exception is thrown.
    /// </summary>
    /// <returns>The current <see cref="User" /> or the guest <see cref="User" />.</returns>
    /// <exception cref="Exception">Thrown if a logged-in user is not found in the database.</exception>
    public async Task<User> GetCurrentUser()
    {
        var currentUser = await userAuthenticationService.GetUserInfo();
        if (currentUser == null)
            return await GetGuestUser();

        var userInDb = await dbContext.Users.FindAsync(currentUser.UserId);
        if (userInDb != null)
            return userInDb;

        _logger.LogError("Unable to find logged in user in database. Email: {}, Username: {}", currentUser.Email,
            currentUser.UserName);
        throw new Exception("Unable to find logged in user");
    }

    public ValueTask<User> GetGuestUser()
    {
        return dbContext.Users.FindAsync(User.GuestUserId)!;
    }
}