using Dipl.Business.Entities;

namespace Dipl.Business.Services;

public class UsersService(AppDbContext dbContext)
{
    public async Task CreateIfNotExists(User user)
    {
        if (await dbContext.Users.FindAsync(user.UserId) != null)
            return;

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
    }
}