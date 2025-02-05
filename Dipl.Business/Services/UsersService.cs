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