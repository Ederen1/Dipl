using Dipl.Business.Services.Interfaces;
using Dipl.Common.Configs;
using Dipl.Common.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Business.Services;

/// <summary>
///     Implements <see cref="IStoreService" /> using the local file system as the backing store.
/// </summary>
public class FileStoreService(IOptions<FileStoreServiceConfiguration> options, ILogger<FileStoreService> logger)
    : IStoreService
{
    private readonly string _basePath = options.Value.BasePath;

    public async Task InsertFile(string fileName, string folder, Stream contents)
    {
        var fullPath = Path.Combine(_basePath, folder, fileName);
        logger.LogInformation("Uploading file: {}", fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!); // Ensure the target directory exists.
        await using var file = File.Open(fullPath, FileMode.Create, FileAccess.Write);
        var transferDone = false;
        try
        {
            // Copy with a larger buffer to potentially improve performance for big files.
            await contents.CopyToAsync(file, 1024 * 1024);
            transferDone = true;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("File transfer was cancelled.");
        }
        finally
        {
            // If the transfer was not completed for some reason, delete the partially written file.
            if (!transferDone)
                File.Delete(fullPath);
        }
    }

    public Task CreateDirectoryIfNotExists(string name)
    {
        var fullPath = _basePath + name;

        Directory.CreateDirectory(fullPath);
        return Task.CompletedTask;
    }

    public Task<Stream> GetFile(string name)
    {
        var fullPath = _basePath + name;

        return Task.FromResult<Stream>(File.Open(fullPath, FileMode.Open, FileAccess.Read));
    }

    public Task DeleteFile(string fileName, string folder)
    {
        var fullPath = Path.Combine(_basePath, folder, fileName);
        File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task DeleteDirectory(string path, bool recursive = false)
    {
        var fullPath = _basePath + path;

        Directory.Delete(fullPath, recursive);
        return Task.CompletedTask;
    }

    public Task<FileInfo[]?> ListFolder(string path)
    {
        var fullPath = _basePath + path;

        var directory = new DirectoryInfo(fullPath);
        if (!directory.Exists)
            return Task.FromResult<FileInfo[]?>(null);

        var found = directory.GetFileSystemInfos().MapToFileInfos(_basePath);
        return Task.FromResult<FileInfo[]?>(found);
    }
}