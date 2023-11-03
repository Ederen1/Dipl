using Dipl.Business.Entities;
using Dipl.Common.Types;

namespace Dipl.Business.Services;

public class LinksService(AppDbContext dbContext)
{
    public async Task<Link> GenerateLink(string folderPath, UserInfo? user)
    {
        User createdBy;
        if (user == null)
            createdBy = await dbContext.Users.FindAsync(User.GuestUserId) ?? throw new Exception("Guest user not found");
        else
            createdBy = await dbContext.Users.FindAsync(user.UserId) ?? throw new Exception("User not found");

        var link = new Link { Folder = folderPath, CreatedBy = createdBy };
        link.Groups.Add(await dbContext.Groups.FindAsync(Group.GuestGrupId) ?? throw new Exception("Guest group not found"));

        await dbContext.Links.AddAsync(link);
        await dbContext.SaveChangesAsync();

        return link;
    }
}