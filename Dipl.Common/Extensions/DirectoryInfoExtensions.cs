using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Common.Extensions;

public static class DirectoryInfoExtensions
{
    public static FileInfo MapToFileInfo(this FileSystemInfo info)
    {
        return new FileInfo
        {
            Path = info.FullName,
            Created = info.CreationTime,
            Updated = info.LastWriteTime,
            IsFolder = info is DirectoryInfo
        };
    }
    
    public static IEnumerable<FileInfo> MapToFileInfos(this IEnumerable<FileSystemInfo> infos)
    {
        return infos.Select(info => info.MapToFileInfo());
    }
}