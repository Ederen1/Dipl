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
    public async Task CreateLink(RequestLinkCreateModel createModel)
    {
        if (createModel.Password != createModel.MatchingPassword)
            throw new Exception("Passwords do not match");

        var createdBy = await usersService.GetCurrentUser();
        var link = new RequestLink
        {
            LinkId = Guid.NewGuid(),
            CreatedById = createdBy.UserId,
            LinkTitle = createModel.LinkName,
            Message = createModel.MessageForUser,
            NotifyOnUpload = createModel.NotifyOnUpload,
            IsProtected = createModel.IsProtected,
            AllFilesSizeLimit = createModel.SizeLimitPremultiplied is { } premultiplied
                ? premultiplied * createModel.SizeLimitMultiplier
                : long.MaxValue,
            AllowedExtensions = createModel.AllowedExtensions,
            UploadSlots = createModel.SendTo.Select(sendto => new RequestLinkUploadSlot
            {
                RequestLinkUploadSlotId = Guid.NewGuid(),
                Email = sendto
            }).ToList()
        };

        if (!string.IsNullOrEmpty(createModel.Password))
            await LinkSecurityService.SetupSecureLinkAsync(createModel.Password, link);

        await storeService.CreateDirectoryIfNotExists(link.LinkId.ToString());

        foreach (var userDirectory in link.UploadSlots)
            await storeService.CreateDirectoryIfNotExists(link.LinkId + "/" + userDirectory.RequestLinkUploadSlotId);

        await dbContext.RequestLinks.AddAsync(link);
        await dbContext.SaveChangesAsync();

        await emailSenderService.NotifyOfRequest(createModel, link, createdBy.UserName);
    }

    /// <summary>
    ///     Sends a notification email when files have been uploaded to a request link slot.
    /// </summary>
    /// <param name="model">The model containing details for the notification email.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task NotifyFileRequestUpload(NotifyRequestUploadedModel model,
        CancellationToken cancellationToken = default)
    {
        await emailSenderService.NotifyUserUploadedToRequestLink(model, cancellationToken);
    }
}