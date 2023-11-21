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

        var link = new Link { Folder = folderPath, CreatedById = createdBy.UserId };
        link.Groups.Add(await dbContext.Groups.FindAsync(Group.GuestGrupId) ?? throw new Exception("Guest group not found"));

        await dbContext.Links.AddAsync(link);
        await dbContext.SaveChangesAsync();

        return link;
    }

    public async Task<Link> Get(Guid linkId)
    {
        return await dbContext.Links.FindAsync(linkId) ?? throw new Exception("Link not found");
    }

    public async Task<Common.Types.FileInfo[]> EnumerateLink(Guid linkId)
    {
        return await EnumerateLink(await Get(linkId));
    }

    public async Task<Common.Types.FileInfo[]> EnumerateLink(Link link)
    {
        return await fileStoreService.List(link.Folder);
    }

    public async Task<Stream> GetFile(Guid linkId, string fileName)
    {
        var link = await dbContext.Links.FindAsync(linkId) ?? throw new Exception("Link not found");
        return await fileStoreService.GetFile($"{link.Folder}/{fileName}");
    }

    public async Task<Link> GetLinkForRequest(User user)
    {
        var link = new Link
        {
            CreatedById = user.UserId,
            LinkType = LinkTypeEnum.Request,
            Folder = $"{user.UserId}/{Guid.NewGuid()}",
        };

        await dbContext.Links.AddAsync(link);
        await dbContext.SaveChangesAsync();
        return link;
    }
}