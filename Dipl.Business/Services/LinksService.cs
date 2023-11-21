using Dipl.Business.Entities;
using Dipl.Business.Models;
using Dipl.Business.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Link> GetLinkForRequest(User user, string folderName, IEnumerable<string> sendTo, string message)
    {
        var link = new Link
        {
            CreatedById = user.UserId,
            LinkType = LinkTypeEnum.Request,
            Folder = $"{user.UserId}/{folderName}",
            Message = message
        };
        
        // TODO: Sending emails

        await fileStoreService.CreateFolder(link.Folder);
        await dbContext.Links.AddAsync(link);
        await dbContext.SaveChangesAsync();
        return link;
    }

    public async Task<IEnumerable<LinkWithListedFiles>> GetLinksForUser(User user)
    {
        var links = await dbContext.Links.Where(x => x.CreatedById == user.UserId).ToListAsync();

        return await Task.WhenAll(links.Select(async link =>
        {
            // TODO: maybe cache this?
            var fileInfos = await fileStoreService.List(link.Folder);
            return LinkWithListedFiles.FromLink(link, fileInfos);
        }));
    }

    public async Task DeleteLink(Guid linkId)
    {
        var cached = await dbContext.Links.FindAsync(linkId);
        if (cached == null)
            throw new Exception("Internal error");
        
        await fileStoreService.DeleteFolder(cached.Folder, true);
        dbContext.Links.Remove(cached);
        await dbContext.SaveChangesAsync();
    }
}