using Dipl.Business.Entities;
using Dipl.Business.Models;
using Dipl.Business.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Dipl.Business.Services;

public class RequestLinksService(
    IStoreService fileStoreService,
    AppDbContext dbContext,
    NavigationManager navigationManager,
    EmailSenderService emailSenderService,
    UsersService usersService)
{
    public async Task<RequestLink> CreateLink(RequestLinkModel model)
    {
        var createdBy = await usersService.GetCurrentUser();
        var link = new RequestLink
        {
            CreatedById = createdBy.UserId,
            Folder = $"{createdBy.UserId}/{Guid.NewGuid()}",
            LinkTitle = model.LinkName,
            Message = model.MessageForUser,
            NotifyOnUpload = model.NotifyOnUpload,
            Permission = (await dbContext.Permissions.FindAsync(Permission.GuestPermissionId))!
        };
        
        await fileStoreService.CreateDirectoryIfNotExists(link.Folder);
        await dbContext.RequestLinks.AddAsync(link);
        await dbContext.SaveChangesAsync();
        
        var linkUrl = navigationManager.ToAbsoluteUri($"/link/request/{link.LinkId}");
        await emailSenderService.NotifyOfRequest(model, link.LinkId);
        
        return link;
    }
}