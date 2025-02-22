using Blazorise;
using Dipl.Business;
using Dipl.Business.Entities;
using Dipl.Business.Services;
using Dipl.Business.Services.Interfaces;
using Dipl.Web.Models;
using Microsoft.EntityFrameworkCore;
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

    public async Task<UploadLink> UploadAllToFolder(FileUploadModel model, UploadLink? uploadLink,
        CancellationToken cancellationToken = default)
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

        await UploadFiles(model.FilesToUpload, link.LinkId.ToString(), cancellationToken);
        await emailSenderService.NotifyUserUploaded(link, mappedModel, cancellationToken);

        return link;
    }

    public async Task RespondToFileRequest(RequestLinkResponseModel model, RequestLinkUploadSlot slot,
        CancellationToken cancellationToken = default)
    {
        var link = slot.RequestLink;
        var dir = link.LinkId + "/" + slot.RequestLinkUploadSlotId;

        await UploadFiles(model.FilesToUpload, dir, cancellationToken);
        slot.Uploaded = DateTime.Now;

        if (link.NotifyOnUpload)
        {
            var notifyModel = model.MapToNotifyRequestUploadedModel(link, slot);
            await requestLinksService.NotifyFileRequestUpload(notifyModel, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UploadFiles(IEnumerable<IFileEntry> files, string folder, CancellationToken cancellationToken)
    {
        var existingFiles = await storeService.ListFolder(folder);
        var (keepUpload, toDelete) = DiffFiles(files, existingFiles);

        foreach (var file  in toDelete)
        {
            await storeService.DeleteFile(file.Name, folder);
        }
        
        foreach (var chunkedFiles in keepUpload.Chunk(UploadChunkSize))
        {
            var tasks = chunkedFiles.Select(file => storeService.InsertFile(file.Name, folder,
                file.OpenReadStream(long.MaxValue, cancellationToken), cancellationToken));

            await Task.WhenAll(tasks);
        }
    }

    private (IEnumerable<IFileEntry> keepUpload, IEnumerable<FileInfo> toDelete) DiffFiles(IEnumerable<IFileEntry> files,
        FileInfo[]? existingFiles)
    {
        if (existingFiles is null)
            return (files, []);

        var keepUpload = files.Where(f => f.Status == FileEntryStatus.Ready).ToList();
        var toRemoveUpdatePart = existingFiles.Where(file => keepUpload.Any(up => up.Name == file.Name));
        // var toRemove = toRemoveUpdatePart.Concat();
        var toRemove = existingFiles.Where(f => !files.Any(file => file.Name == f.Name));
        
        return (keepUpload, toRemove);
    }
}