﻿using Dipl.Business.Services.Interfaces;
using Dipl.Common.Extensions;
using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Business.Services;

public class FileStoreService(string basePath) : IStoreService
{
    public async Task InsertFile(string filePath, Stream contents, Action<long> progress, CancellationToken cancellationToken = default)
    {
        var fullPath = basePath + filePath;

        await using var file = File.Open(fullPath, FileMode.Create, FileAccess.Write);

        var buffer = new byte[1024 * 1024];
        int read = 0;
        while (!cancellationToken.IsCancellationRequested && (read = await contents.ReadAsync(buffer, cancellationToken)) != 0)
        {
            await file.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            progress(file.Length);
        }
    }

    public Task CreateFolder(string name)
    {
        var fullPath = basePath + name;

        Directory.CreateDirectory(fullPath);
        return Task.CompletedTask;
    }

    public Task<bool> FolderExists(string name)
    {
        var fullPath = basePath + name;

        return Task.FromResult(Directory.Exists(fullPath));
    }

    public Task<Stream> GetFile(string name)
    {
        var fullPath = basePath + name;

        return Task.FromResult<Stream>(File.Open(fullPath, FileMode.Open, FileAccess.Read));
    }

    public Task Delete(string path)
    {
        var fullPath = basePath + path;
        File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task DeleteFolder(string path, bool recursive = false)
    {
        var fullPath = basePath + path;

        Directory.Delete(fullPath, recursive);
        return Task.CompletedTask;
    }

    public Task Move(string source, string dest)
    {
        var fullSource = basePath + source;
        var fullDest = basePath + source;

        File.Move(fullSource, fullDest);

        return Task.CompletedTask;
    }

    public Task<FileInfo[]> List(string path)
    {
        var fullPath = basePath + path;

        var directory = new DirectoryInfo(fullPath);
        var found = directory.GetFileSystemInfos().MapToFileInfos();

        return Task.FromResult(found);
    }

    public Task<FileInfo[]> Search(string name)
    {
        var directory = new DirectoryInfo(basePath);
        var found = directory.GetFileSystemInfos(name, SearchOption.AllDirectories).MapToFileInfos();

        return Task.FromResult(found);
    }
}