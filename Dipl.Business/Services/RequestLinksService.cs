using Dipl.Business.EmailModels;
using Dipl.Business.Entities;
using Dipl.Business.Models;
using Dipl.Business.Services.Interfaces;

namespace Dipl.Business.Services;

public class RequestLinksService(
    IStoreService storeService,
    AppDbContext dbContext,
    EmailSenderService emailSenderService,
    UsersService usersService)
{
    public async Task<RequestLink> CreateLink(RequestLinkCreateModel createModel)
    {
        var createdBy = await usersService.GetCurrentUser();
        var link = new RequestLink
        {
            LinkId = Guid.NewGuid(),
            CreatedById = createdBy.UserId,
            LinkTitle = createModel.LinkName,
            Message = createModel.MessageForUser,
            NotifyOnUpload = createModel.NotifyOnUpload,
            UploadSlots = createModel.SendTo.Select(sendto => new RequestLinkUploadSlot
            {
                RequestLinkUploadSlotId = Guid.NewGuid(),
                Email = sendto,
            }).ToList()
        };

        await storeService.CreateDirectoryIfNotExists(link.LinkId.ToString());
        
        foreach (var userDirectory in link.UploadSlots)
            await storeService.CreateDirectoryIfNotExists(link.LinkId + "/" + userDirectory.RequestLinkUploadSlotId);
        
        await dbContext.RequestLinks.AddAsync(link);
        await dbContext.SaveChangesAsync();

        await emailSenderService.NotifyOfRequest(createModel, link, createdBy.UserName);

        return link;
    }

    public async Task NotifyFileRequestUpload(NotifyRequestUploadedModel model,
        CancellationToken cancellationToken = default)
    {
        await emailSenderService.NotifyUserUploadedToRequestLink(model, cancellationToken);
    }
}