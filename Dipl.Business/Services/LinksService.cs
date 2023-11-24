using Dipl.Business.Entities;
using Dipl.Business.Models;
using Dipl.Business.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Dipl.Business.Services;

public class LinksService(AppDbContext dbContext, EmailSenderService emailSenderService, NavigationManager navigationManager, IStoreService fileStoreService)
{
    public async Task<Link> GenerateLink(string folderPath, User? user)
    {
        User createdBy;
        if (user == null)
            createdBy = await dbContext.Users.FindAsync(User.GuestUserId) ?? throw new Exception("Guest user not found");
        else
            createdBy = await dbContext.Users.FindAsync(user.UserId) ?? throw new Exception("User not found");

        var guestPermission = await dbContext.Permissions.FindAsync(Permission.GuestPermissionId);
        var link = new Link { Folder = folderPath, CreatedById = createdBy.UserId, Permission = guestPermission!};

        await dbContext.Links.AddAsync(link);
        await dbContext.SaveChangesAsync();

        return link;
    }

    public async Task<Link> Get(Guid linkId)
    {
        return await dbContext.Links.FindAsync(linkId) ?? throw new Exception("Link not found");
    }
    
    public async Task<Stream> GetFile(Guid linkId, string fileName)
    {
        var link = await dbContext.Links.FindAsync(linkId) ?? throw new Exception("Link not found");
        return await fileStoreService.GetFile($"{link.Folder}/{fileName}");
    }

    public async Task<Link> GenerateRequestAndSendEmail(User user, RequestLinkModel request)
    {
        var link = new Link
        {
            CreatedById = user.UserId,
            LinkType = LinkTypeEnum.Request,
            Folder = $"{user.UserId}/{Guid.NewGuid()}",
            LinkName = request.LinkName,
            Message = request.MessageForUser,
            NotifyOnUpload = request.NotifyOnUpload,
            Permission = (await dbContext.Permissions.FindAsync(Permission.GuestPermissionId))!
        };

        await fileStoreService.CreateFolder(link.Folder);
        await dbContext.Links.AddAsync(link);
        await dbContext.SaveChangesAsync();
        
        var linkUrl = navigationManager.ToAbsoluteUri($"/link/request/{link.LinkId}");        
        await emailSenderService.NotifyOfRequest(request, user.UserName, linkUrl.ToString());
        
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
        var inDb = await dbContext.Links.FindAsync(linkId);
        if (inDb == null)
            throw new Exception("Internal error");
        
        await fileStoreService.DeleteFolder(inDb.Folder, true);
        dbContext.Links.Remove(inDb);
        await dbContext.SaveChangesAsync();
    }

    public async Task CloseLink(Guid linkId, string? uploader)
    {
        var link = await dbContext.Links.FindAsync(linkId) ?? throw new Exception("Link not found");
        
        await CloseLink(link, uploader);
    }
    
    public async Task CloseLink(Link link, string? uploader)
    {
        link.LinkClosed = true;
        await dbContext.SaveChangesAsync();
        
        if (link.NotifyOnUpload)
            await emailSenderService.NotifyUserUploaded(link, uploader);
    }

    public async Task CreateTemporaryLink(Guid linkId, User? user)
    {
        var linkFolder = (user != null ? user.UserId.ToString() : Guid.NewGuid().ToString()) + "/" + Guid.NewGuid();
        var permission = await dbContext.Permissions.FindAsync(Permission.GuestPermissionId);
        var link = new Link
        {
            LinkId = linkId,
            LinkType = LinkTypeEnum.Temporary,
            Permission = permission!,
            Folder = linkFolder,
            CreatedById = user?.UserId ?? User.GuestUserId
        };
        
        await fileStoreService.CreateFolder(linkFolder);
        await dbContext.Links.AddAsync(link);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateLink(Guid linkId, UploadLinkModel model)
    {
        var inDb = await dbContext.Links.FindAsync(linkId);
        if (inDb == null)
            throw new Exception("Link not found");
        
        inDb.Message = model.MessageForUser;
        inDb.NotifyOnUpload = model.NotifyOnUpload;
        inDb.LinkName = model.LinkName;
        await dbContext.SaveChangesAsync();
    }
}