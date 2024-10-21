using Dipl.Business.Entities;

namespace Dipl.Business.Services;

public class InitializationService(AppDbContext dbContext)
{
    public async Task Initialize()
    {
        var group = new Group { GroupId = Group.GuestGrupId, Name = "Guest" };
        await dbContext.Groups.AddAsync(group);
        await dbContext.Permissions.AddAsync(
            new Permission
            {
                PermissionId = Permission.GuestPermissionId,
                Group = group,
                Read = true,
                Write = false
            }
        );
        await dbContext.Users.AddAsync(
            new User
            {
                UserId = User.GuestUserId,
                Email = "guest@example.com",
                UserName = "Guest",
                Groups = new List<Group> { group }
            }
        );

        await dbContext.SaveChangesAsync();
    }
}