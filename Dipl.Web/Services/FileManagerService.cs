using Blazorise;
using Dipl.Business;
using Dipl.Business.Services;
using Dipl.Business.Services.Interfaces;
using Dipl.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Dipl.Web.Services;

public class FileManagerService(
    IStoreService storeService,
    UserAuthenticationService userAuthenticationService,
    UploadLinksService uploadLinksService,
    RequestLinksService requestLinksService,
    AppDbContext dbContext,
    ILogger<FileManagerService> logger)
{
    private const int UploadChunkSize = 4;

    public async Task<Guid> UploadAllToFolder(FileUploadModel model, CancellationToken cancellationToken = default)
    {
        var userEmail = (await userAuthenticationService.GetUserInfo())?.Email;
        var mappedModel = model.MapToCreateUploadModel(userEmail);

        await UploadFiles(model.FilesToUpload, mappedModel.FullFolderName, cancellationToken);

        return await uploadLinksService.GenerateLinkAfterUploadAndNotifyUser(mappedModel);
    }

    public async Task RespondToFileRequest(RequestLinkResponseModel model,
        CancellationToken cancellationToken = default)
    {
        var link = await dbContext.RequestLinks
            .FirstOrDefaultAsync(x => x.LinkId == model.LinkId, cancellationToken);

        if (link is null)
        {
            logger.LogError("Link by id: '{}' not found in database", model.LinkId);
            return;
        }

        await UploadFiles(model.FilesToUpload, link.Folder, cancellationToken);

        if (link.NotifyOnUpload)
        {
            var notifyModel = model.MapToNotifyRequestUploadedModel(link.LinkTitle, link.CreatedBy.Email);
            await requestLinksService.NotifyFileRequestUpload(notifyModel, cancellationToken);
        }

        var slot = link.UploadSlots.FirstOrDefault(slot => slot.Email == model.ResponderEmail);
        if (slot == null)
        {
            logger.LogError("Slot for link {} not found", link.Folder);
            return;
        }

        slot.Closed = true;
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