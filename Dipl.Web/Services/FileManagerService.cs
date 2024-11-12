using Dipl.Business.Services;
using Dipl.Business.Services.Interfaces;
using Dipl.Web.Models;

namespace Dipl.Web.Services;

public class FileManagerService(IStoreService storeService, UserAuthenticationService userAuthenticationService, UploadLinksService uploadLinksService)
{
    // public async Task<string> SetupFolderForUpload(string folderName, User? userInfo)
    // {
    //     var userOrTemporaryFolderName = userInfo?.UserId ?? Guid.NewGuid().ToString();
    //     var realFolderName = await GetFolderNameIfEmpty(userOrTemporaryFolderName, folderName);
    //     var fullFolderName = $"{userOrTemporaryFolderName}/{realFolderName}";
    //
    //     await storeService.CreateFolder(userOrTemporaryFolderName);
    //     await storeService.CreateFolder(fullFolderName);
    //
    //     return fullFolderName;
    // }
    //
    // public async Task<string> GetFolderNameIfEmpty(
    //     string userOrTemporaryFolderName,
    //     string? folderName
    // )
    // {
    //     if (!string.IsNullOrWhiteSpace(folderName)) return folderName;
    //
    //     var maxTries = 10;
    //     var tries = 0;
    //     do
    //     {
    //         var tryFoldername = Random.Shared.Next() % 100 + tries;
    //         var fullName = userOrTemporaryFolderName + "/" + tryFoldername;
    //
    //         if (!await storeService.FolderExists(fullName))
    //             return tryFoldername.ToString();
    //
    //         tries--;
    //     } while (tries < maxTries);
    //
    //     throw new Exception($"Random folder already exists, {maxTries} tries exhausted");
    // }

    // public async Task DeleteFile(string folder, string fileName)
    // {
    //     await storeService.Delete($"{folder}/{fileName}");
    // }
    //
    // public async Task CreateFolder(string folder)
    // {
    //     await storeService.CreateFolder(folder);
    // }

    public async Task UploadAllToFolder(FileUploadModel model, CancellationToken cancellationToken = default)
    {
        var userEmail = (await userAuthenticationService.GetUserInfo())?.Email;
        var mappedModel = model.MapToCreateUploadModel(userEmail);

        const int chunkSize = 4;
        foreach (var files in model.FilesToUpload.Chunk(chunkSize))
        {
            var tasks = files.Select(file => storeService.InsertFile(file.Name, mappedModel.FullFolderName,
                file.OpenReadStream(long.MaxValue, cancellationToken), cancellationToken));

            await Task.WhenAll(tasks);
        }

        await uploadLinksService.GenerateLinkAfterUploadAndNotifyUser(mappedModel);
    }
}