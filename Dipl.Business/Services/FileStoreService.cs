using Dipl.Business.Services.Interfaces;
using Dipl.Common.Configs;
using Dipl.Common.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Business.Services;

public class FileStoreService(IOptions<FileStoreConfiguration> options, ILogger<FileStoreService> logger)
    : IStoreService
{
    private readonly string _basePath = options.Value.BasePath;

    public async Task InsertFile(string fileName, string folder, Stream contents,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, folder, fileName);
        logger.LogInformation("Uploading file: {}", fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var file = File.Open(fullPath, FileMode.Create, FileAccess.Write);
        var transferDone = false;
        try
        {
            await contents.CopyToAsync(file, cancellationToken);
            transferDone = true;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("File transfer was cancelled.");
        }
        finally
        {
            await file.DisposeAsync();
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

    public Task<bool> DirectoryExists(string name)
    {
        var fullPath = _basePath + name;

        return Task.FromResult(Directory.Exists(fullPath));
    }

    public Task<Stream> GetFile(string name)
    {
        var fullPath = _basePath + name;

        return Task.FromResult<Stream>(File.Open(fullPath, FileMode.Open, FileAccess.Read));
    }

    public Task Delete(string path)
    {
        var fullPath = _basePath + path;
        File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task DeleteDirectory(string path, bool recursive = false)
    {
        var fullPath = _basePath + path;

        Directory.Delete(fullPath, recursive);
        return Task.CompletedTask;
    }

    public Task Move(string source, string dest)
    {
        var fullSource = _basePath + source;
        var fullDest = _basePath + source;

        File.Move(fullSource, fullDest);

        return Task.CompletedTask;
    }

    public Task<FileInfo[]> ListFolder(string path)
    {
        var fullPath = _basePath + path;

        var directory = new DirectoryInfo(fullPath);
        var found = directory.GetFileSystemInfos().MapToFileInfos(_basePath);

        return Task.FromResult(found);
    }

    public Task<FileInfo[]> Search(string name)
    {
        var directory = new DirectoryInfo(_basePath);
        var found = directory.GetFileSystemInfos(name, SearchOption.AllDirectories).MapToFileInfos(_basePath);

        return Task.FromResult(found);
    }
}