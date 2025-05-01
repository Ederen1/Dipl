using Blazorise;
using Dipl.Business;
using Dipl.Business.EmailModels;
using Dipl.Business.Entities;
using Dipl.Business.Models;
using Dipl.Business.Services;
using Dipl.Business.Services.Interfaces;
using Dipl.Web.Models;
using Microsoft.JSInterop;
using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Web.Services;

public class FileManagerService(
    IStoreService storeService,
    EmailSenderService emailSenderService,
    UsersService usersService,
    RequestLinksService requestLinksService,
    AppDbContext dbContext,
    IJSRuntime jsRuntime)
{
    private const int UploadChunkSize = 4;

    public async Task<(UploadLink, List<FileInfo>)> UploadAllToFolder(FileUploadModel model, UploadLink? uploadLink,
        List<FileInfo> alreadyPresentFiles, CancellationToken cancellationToken = default)
    {
        if (model.Password != model.MatchingPassword)
            throw new Exception("Passwords do not match");

        var createdBy = await usersService.GetCurrentUser();
        var mappedModel = model.MapToCreateUploadModel(createdBy.UserName);

        var link = uploadLink ?? new UploadLink();
        link.CreatedById = createdBy.UserId;
        link.LinkTitle = model.LinkTitle;
        link.Message = model.MessageForUser;
        link.Uploaded = DateTime.Now;

        if (uploadLink is null)
            await dbContext.UploadLinks.AddAsync(link, cancellationToken);

        var folder = link.LinkId.ToString();
        if (uploadLink is not null)
            await DeleteFilesBeforeInsert(folder, alreadyPresentFiles);

        if (uploadLink is null && !string.IsNullOrEmpty(model.Password))
            await LinkSecurityService.SetupSecureLinkAsync(model.Password, link);

        await dbContext.SaveChangesAsync(cancellationToken);
        var folderContents = await UploadFiles(link.LinkId, null, model.Password, cancellationToken);
        if (folderContents.Count == 0)
            throw new Exception($"Folder for link {link.LinkTitle} '{link.LinkId}' is empty. This should never happen");

        await emailSenderService.NotifyUserUploaded(link, mappedModel, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (link, folderContents.ToList());
    }

    public async Task<List<FileInfo>> RespondToFileRequest(RequestLinkUploadSlot slot, List<IFileEntry> filesToUpload,
        List<FileInfo> alreadyPresentFiles, string password, CancellationToken cancellationToken = default)
    {
        var link = slot.RequestLink;
        var dir = link.LinkId + "/" + slot.RequestLinkUploadSlotId;

        await DeleteFilesBeforeInsert(dir, alreadyPresentFiles);

        var files = await UploadFiles(link.LinkId, slot.RequestLinkUploadSlotId, password, cancellationToken);
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

    private async Task DeleteFilesBeforeInsert(string folder, List<FileInfo> alreadyPresentFiles)
    {
        var existingFiles = await storeService.ListFolder(folder);
        var toDelete = existingFiles?.Where(f => alreadyPresentFiles.All(apf => apf.Name != f.Name)).ToList() ?? [];

        foreach (var file in toDelete)
            await storeService.DeleteFile(file.Name, folder);
    }

    private async Task<List<FileInfo>> UploadFiles(Guid linkId, Guid? slotId, string? password,
        CancellationToken cancellationToken)
    {
        await jsRuntime.InvokeVoidAsync("window.uploadFiles", cancellationToken, linkId, slotId, password);

        var folder = slotId is null ? linkId.ToString() : $"{linkId}/{slotId}";
        return (await storeService.ListFolder(folder))?.ToList() ?? [];
    }
}