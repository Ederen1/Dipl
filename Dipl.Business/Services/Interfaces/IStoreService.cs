﻿using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Business.Services.Interfaces;

public interface IStoreService
{
    public Task InsertFile(string fileName, string folder, Stream contents,
        CancellationToken cancellationToken = default);

    public Task CreateDirectoryIfNotExists(string name);
    public Task<bool> DirectoryExists(string name);
    public Task<Stream> GetFile(string name);
    public Task DeleteFile(string fileName, string folder);
    public Task DeleteDirectory(string path, bool recursive = false);
    public Task DeleteDirectoryContents(string path);
    public Task Move(string source, string dest);
    public Task<FileInfo[]?> ListFolder(string path);
    public Task<FileInfo[]> Search(string name);
}