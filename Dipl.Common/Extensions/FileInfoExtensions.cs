using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Common.Extensions;

public static class FileInfoExtensions
{
    public static FileInfo MapToFileInfo(this FileSystemInfo info, string basePath)
    {
        return new FileInfo
        {
            Path = RemoveBeforeSubstring(info.FullName, basePath),
            Created = info.CreationTime,
            Updated = info.LastWriteTime,
            IsFolder = info is DirectoryInfo,
            Size = info is System.IO.FileInfo fileInfo ? fileInfo.Length : 0
        };
    }

    private static string RemoveBeforeSubstring(string input, string substring)
    {
        var index = input.IndexOf(substring, StringComparison.Ordinal);
        return index >= 0 ? 
            input[(index + substring.Length)..] : input; 
    }

    public static IEnumerable<FileInfo> MapToFileInfos(this IEnumerable<FileSystemInfo> infos, string basePath)
    {
        return infos.Select(info => info.MapToFileInfo(basePath));
    }

    public static FileInfo[] MapToFileInfos(this FileSystemInfo[] infos, string basePath)
    {
        return MapToFileInfos(infos.AsEnumerable(), basePath).ToArray();
    }
}