using Dipl.Business.Entities;
using Dipl.Business.Models;
using Dipl.Business.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Dipl.Business.Services;

public class UploadLinksService(
    AppDbContext dbContext,
    EmailSenderService emailSenderService,
    UsersService usersService,
    IStoreService fileStoreService)
{
    public async Task<Guid> GenerateLinkAfterUploadAndNotifyUser(CreateUploadLinkModel model)
    {
        var createdBy = await usersService.GetCurrentUser();
        var guestPermission = await dbContext.Permissions.FindAsync(Permission.GuestPermissionId);

        var link = new UploadLink
        {
            Folder = model.FullFolderName,
            CreatedById = createdBy.UserId,
            Permission = guestPermission!,
            Message = model.MessageForUser,
            LinkTitle = model.LinkTitle
        };

        await dbContext.UploadLinks.AddAsync(link);
        await dbContext.SaveChangesAsync();

        await emailSenderService.NotifyUserUploaded(link, model);
        return link.LinkId;
    }

    public async Task<Stream> GetFile(Guid linkId, string fileName)
    {
        var link = await dbContext.UploadLinks.FindAsync(linkId) ?? throw new Exception("Link not found");
        return await fileStoreService.GetFile($"{link.Folder}/{fileName}");
    }
}