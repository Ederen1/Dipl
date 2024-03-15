using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Common.Extensions;

public static class FileInfoExtensions
{
    public static FileInfo MapToFileInfo(this FileSystemInfo info)
    {
        return new FileInfo
        {
            Path = info.FullName,
            Created = info.CreationTime,
            Updated = info.LastWriteTime,
            IsFolder = info is DirectoryInfo,
            Size = info is System.IO.FileInfo fileInfo ? fileInfo.Length : 0
        };
    }

    public static IEnumerable<FileInfo> MapToFileInfos(this IEnumerable<FileSystemInfo> infos)
    {
        return infos.Select(info => info.MapToFileInfo());
    }

    public static FileInfo[] MapToFileInfos(this FileSystemInfo[] infos)
    {
        return infos.Select(info => info.MapToFileInfo()).ToArray();
    }
}
