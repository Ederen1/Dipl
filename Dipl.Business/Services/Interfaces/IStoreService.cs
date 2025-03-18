using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Business.Services.Interfaces;

public interface IStoreService
{
    Task InsertFile(string fileName, string folder, Stream contents);
    Task CreateDirectoryIfNotExists(string name);
    Task<Stream> GetFile(string name);
    Task DeleteFile(string fileName, string folder);
    Task DeleteDirectory(string path, bool recursive = false);
    Task<FileInfo[]?> ListFolder(string path);
}