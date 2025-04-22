using Blazorise;
using Dipl.Business;
using Dipl.Business.EmailModels;
using Dipl.Business.Entities;
using Dipl.Business.Models;
using Dipl.Business.Services;
using Dipl.Business.Services.Interfaces;
using Dipl.Web.Models;
using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Web.Services;

public class FileManagerService(
    IStoreService storeService,
    EmailSenderService emailSenderService,
    UsersService usersService,
    RequestLinksService requestLinksService,
    AppDbContext dbContext)
{
    private const int UploadChunkSize = 4;

    public async Task<(UploadLink, List<FileInfo>)> UploadAllToFolder(FileUploadModel model, UploadLink? uploadLink,
        List<FileInfo> alreadyPresentFiles, CancellationToken cancellationToken = default)
    {
        var createdBy = await usersService.GetCurrentUser();
        var mappedModel = model.MapToCreateUploadModel(createdBy.UserName);

        var link = uploadLink ?? new UploadLink();
        link.CreatedById = createdBy.UserId;
        link.LinkTitle = model.LinkTitle;
        link.Message = model.MessageForUser;
        link.Uploaded = DateTime.Now;

        if (uploadLink is null)
            await dbContext.UploadLinks.AddAsync(link, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        await UploadFiles(model.FilesToUpload, alreadyPresentFiles, link.LinkId.ToString(), cancellationToken);
        await emailSenderService.NotifyUserUploaded(link, mappedModel, cancellationToken);

        var folderContents = await storeService.ListFolder($"{link.LinkId}");
        if (folderContents is null)
            throw new Exception($"Folder for link {link.LinkTitle} '{link.LinkId}' is empty. This should never happen");

        return (link, folderContents.ToList());
    }

    public async Task<List<FileInfo>> RespondToFileRequest(RequestLinkUploadSlot slot, List<IFileEntry> filesToUpload,
        List<FileInfo> alreadyPresentFiles, CancellationToken cancellationToken = default)
    {
        var link = slot.RequestLink;
        var dir = link.LinkId + "/" + slot.RequestLinkUploadSlotId;

        var files = await UploadFiles(filesToUpload, alreadyPresentFiles, dir, cancellationToken);
        slot.Uploaded = DateTime.Now;

        if (link.NotifyOnUpload)
        {
            var notifyModel = new NotifyRequestUploadedModel
            {
                Message = slot.Message ?? "",
                LinkTitle = link.LinkTitle!,
                Link = link,
                UploadSlot = slot,
                EmailTo = link.CreatedBy.Email,
                Files = files.Select(file => new FileInfoModel
                {
                    Name = file.Name,
                    Size = file.Size
                }).ToArray()
            };

            await requestLinksService.NotifyFileRequestUpload(notifyModel, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        var folderContents = await storeService.ListFolder($"{link.LinkId}/{slot.RequestLinkUploadSlotId}");
        if (folderContents is null)
            throw new Exception($"Folder for link {link.LinkTitle} '{link.LinkId}' is empty. This should never happen");

        return folderContents.ToList();
    }

    private async Task<List<FileInfo>> UploadFiles(List<IFileEntry> files, List<FileInfo> alreadyPresentFiles,
        string folder, CancellationToken cancellationToken)
    {
        var existingFiles = await storeService.ListFolder(folder);
        var toDelete = existingFiles?.Where(f => alreadyPresentFiles.All(apf => apf.Name != f.Name)).ToList() ?? [];

        foreach (var file in toDelete)
            await storeService.DeleteFile(file.Name, folder);

        foreach (var chunkedFiles in files.Chunk(UploadChunkSize))
        {
            var tasks = chunkedFiles.Select(file => storeService.InsertFile(file.Name, folder,
                file.OpenReadStream(long.MaxValue, cancellationToken)));

            await Task.WhenAll(tasks);
        }

        return (await storeService.ListFolder(folder))?.ToList() ?? [];
    }
}