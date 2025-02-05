using Dipl.Business.EmailModels;
using Dipl.Business.Entities;
using Dipl.Business.Models;
using Dipl.Business.Services.Interfaces;

namespace Dipl.Business.Services;

public class RequestLinksService(
    IStoreService fileStoreService,
    AppDbContext dbContext,
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
            Permission = (await dbContext.Permissions.FindAsync(Permission.GuestPermissionId))!,
            UploadSlots = model.SendTo.Select(sendto => new RequestLinkUploadSlot
            {
                Closed = false,
                Email = sendto,
            }).ToList()
        };

        foreach (var userDirectory in link.UploadSlots)
            await fileStoreService.CreateDirectoryIfNotExists(link.Folder + "/" + userDirectory.Email);
        
        await fileStoreService.CreateDirectoryIfNotExists(link.Folder);
        await dbContext.RequestLinks.AddAsync(link);
        await dbContext.SaveChangesAsync();

        await emailSenderService.NotifyOfRequest(model, link.LinkId);

        return link;
    }

    public async Task NotifyFileRequestUpload(NotifyRequestUploadedModel model,
        CancellationToken cancellationToken = default)
    {
        await emailSenderService.NotifyUserUploadedToRequestLink(model, cancellationToken);
    }
}