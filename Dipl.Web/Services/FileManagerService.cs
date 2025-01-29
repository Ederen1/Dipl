using Blazorise;
using Dipl.Business;
using Dipl.Business.Models;
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
    AppDbContext dbContext)
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
        var link = await dbContext.RequestLinks.Include(x => x.CreatedBy)
            .FirstOrDefaultAsync(x => x.LinkId == model.LinkId, cancellationToken);
        // TODO: Fix
        if (link is null)
            throw new Exception($"Link {model.LinkId} not found in database");

        await UploadFiles(model.FilesToUpload, link.Folder, cancellationToken);

        if (link.NotifyOnUpload)
        {
            var notifyModel = model.MapToNotifyRequestUploadedModel(link.LinkTitle, link.CreatedBy.Email);
            await requestLinksService.NotifyFileRequestUpload(notifyModel, cancellationToken);
        }
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