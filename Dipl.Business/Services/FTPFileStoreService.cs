using Dipl.Business.Services.Interfaces;
using Dipl.Common.Configs;
using FluentFTP;
using Microsoft.Extensions.Options;
using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Business.Services;

public class FTPFileStoreService : IStoreService
{
    private readonly AsyncFtpClient _client;
    private readonly string BasePath;

    public FTPFileStoreService(IOptions<FTPFileStoreServiceConfiguration> options)
    {
        _client = new AsyncFtpClient(options.Value.Host, options.Value.Username, options.Value.Password,
            options.Value.Port ?? 0);

        _client.Config.EncryptionMode = FtpEncryptionMode.Explicit;
        BasePath = options.Value.BasePath;
        _client.AutoConnect();
    }

    public async Task InsertFile(string fileName, string folder, Stream contents)
    {
        var name = Path.Combine(BasePath, folder, fileName);
        await _client.UploadStream(contents, name, FtpRemoteExists.Overwrite, true);
    }

    public async Task CreateDirectoryIfNotExists(string name)
    {
        var dir = Path.Combine(BasePath, name);
        await _client.CreateDirectory(dir, true);
    }

    public async Task<Stream> GetFile(string name)
    {
        var path = Path.Combine(BasePath, name);
        var stream = new MemoryStream();
        await _client.DownloadStream(stream, path);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public async Task DeleteFile(string fileName, string folder)
    {
        var path = Path.Combine(BasePath, folder, fileName);
        await _client.DeleteFile(path);
    }

    public async Task DeleteDirectory(string path, bool recursive = false)
    {
        var fullPath = Path.Combine(BasePath, path);
        await _client.DeleteDirectory(fullPath, recursive ? FtpListOption.Recursive : FtpListOption.Auto);
    }

    public async Task<FileInfo[]?> ListFolder(string path)
    {
        var fullPath = Path.Combine(BasePath, path);
        var resp = await _client.GetListing(fullPath);

        return resp.Select(item => new FileInfo
        {
            Path = item.FullName,
            Size = item.Size
        }).ToArray();
    }
}