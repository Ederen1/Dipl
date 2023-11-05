using Dipl.Business.Entities;
using Dipl.Business.Services.Interfaces;

namespace Dipl.Business.Services;

public class LinksService(AppDbContext dbContext, IStoreService fileStoreService)
{
    public async Task<Link> GenerateLink(string folderPath, User? user)
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

    public async Task<Common.Types.FileInfo[]> EnumerateLink(Guid linkId)
    {
        var link = await dbContext.Links.FindAsync(linkId) ?? throw new Exception("Link not found");
        return await fileStoreService.List(link.Folder);
    }

    public async Task<Stream> GetFile(Guid linkId, string fileName)
    {
        var link = await dbContext.Links.FindAsync(linkId) ?? throw new Exception("Link not found");
        return await fileStoreService.GetFile($"{link.Folder}/{fileName}");
    }
}