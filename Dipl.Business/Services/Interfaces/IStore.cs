using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Business.Services.Interfaces;

public interface IStore
{
    public Task InsertFile(string filePath, Stream contents);
    public Task CreateFolder(string name);
    public Task<Stream> GetFile(string name);
    public Task Delete(string path);

    public Task DeleteFolder(string path, bool recursive = false);
    public Task Move(string source, string dest);
    public Task<IEnumerable<FileInfo>> List(string path);
    public Task<IEnumerable<FileInfo>> Search(string name);
}