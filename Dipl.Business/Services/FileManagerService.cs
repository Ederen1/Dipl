using Dipl.Business.Entities;
using Dipl.Business.Services.Interfaces;
using Dipl.Common.Types;

namespace Dipl.Business.Services;

public class FileManagerService(IStoreService storeService)
{
    public async Task<string> SetupFolderForUpload(string folderName, User? userInfo)
    {
        var userOrTemporaryFolderName = userInfo?.UserId.ToString() ?? Guid.NewGuid().ToString();
        var realFolderName = await GetFolderNameIfEmpty(userOrTemporaryFolderName, folderName);
        var fullFolderName = $"{userOrTemporaryFolderName}/{realFolderName}";

        await storeService.CreateFolder(userOrTemporaryFolderName);
        await storeService.CreateFolder(fullFolderName);

        return fullFolderName;
    }

    public async Task<string> GetFolderNameIfEmpty(string userOrTemporaryFolderName, string? folderName)
    {
        if (!string.IsNullOrWhiteSpace(folderName))
        {
            return folderName;
        }

        var maxTries = 10;
        var tries = 0;
        do
        {
            var tryFoldername = Random.Shared.Next() % 100 + tries;
            var fullName = userOrTemporaryFolderName + "/" + tryFoldername;

            if (!await storeService.FolderExists(fullName))
                return tryFoldername.ToString();

            tries--;

        } while (tries < maxTries);

        throw new Exception($"Random folder already exists, {maxTries} tries exhausted");
    }

    public async Task UploadFile(string folderName, string fileName, Stream fileStream, CancellationToken cancellationToken = default)
    {
        await storeService.InsertFile($"{folderName}/${fileName}", fileStream, cancellationToken);
    }
}