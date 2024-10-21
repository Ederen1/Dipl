using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Business.Services.Interfaces;

public interface IStoreService
{
    public Task InsertFile(
        string filePath,
        Stream contents,
        Action<long> progress,
        CancellationToken cancellationToken = default
    );

    public Task CreateFolder(string name);
    public Task<bool> FolderExists(string name);
    public Task<Stream> GetFile(string name);
    public Task Delete(string path);
    public Task DeleteFolder(string path, bool recursive = false);
    public Task Move(string source, string dest);
    public Task<FileInfo[]> List(string path);
    public Task<FileInfo[]> Search(string name);
}