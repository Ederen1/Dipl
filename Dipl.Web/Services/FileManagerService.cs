using Blazorise;
using Dipl.Business;
using Dipl.Business.Entities;
using Dipl.Business.Services;
using Dipl.Business.Services.Interfaces;
using Dipl.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Dipl.Web.Services;

public class FileManagerService(
    IStoreService storeService,
    EmailSenderService emailSenderService,
    UsersService usersService,
    RequestLinksService requestLinksService,
    AppDbContext dbContext,
    ILogger<FileManagerService> logger)
{
    private const int UploadChunkSize = 4;

    public async Task<Guid> UploadAllToFolder(FileUploadModel model, CancellationToken cancellationToken = default)
    {
        var createdBy = await usersService.GetCurrentUser();
        var mappedModel = model.MapToCreateUploadModel(createdBy.UserName);

        var link = new UploadLink
        {
            CreatedById = createdBy.UserId,
            Message = model.MessageForUser,
            LinkTitle = model.LinkTitle
        };

        await dbContext.UploadLinks.AddAsync(link, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        await UploadFiles(model.FilesToUpload, link.LinkId.ToString(), cancellationToken);

        await emailSenderService.NotifyUserUploaded(link, mappedModel, cancellationToken);
        return link.LinkId;
    }

    public async Task RespondToFileRequest(RequestLinkResponseModel model, RequestLinkUploadSlot slot,
        CancellationToken cancellationToken = default)
    {
        var link = slot.RequestLink;
        await UploadFiles(model.FilesToUpload, link.LinkId + "/" + slot.RequestLinkUploadSlotId, cancellationToken);
        slot.Closed = true;

        if (link.NotifyOnUpload)
        {
            var notifyModel = model.MapToNotifyRequestUploadedModel(link, slot);
            await requestLinksService.NotifyFileRequestUpload(notifyModel, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UploadFiles(IEnumerable<IFileEntry> files, string folder, CancellationToken cancellationToken)
    {
        foreach (var chunkedFiles in files.Chunk(UploadChunkSize))
        {
            var tasks = chunkedFiles.Select(file => storeService.InsertFile(file.Name, folder,
                file.OpenReadStream(long.MaxValue, cancellationToken), cancellationToken));

            await Task.WhenAll(tasks);
        }
    }
}