using FileInfo = Dipl.Common.Types.FileInfo;

namespace Dipl.Common.Extensions;

public static class FileInfoExtensions
{
    /// <summary>
    ///     Maps a <see cref="FileSystemInfo" /> object to a custom <see cref="FileInfo" /> type.
    ///     The path in the resulting <see cref="FileInfo" /> is made relative to the provided <paramref name="basePath" />.
    /// </summary>
    /// <param name="info">The <see cref="FileSystemInfo" /> to map.</param>
    /// <param name="basePath">The base path to remove from the full path of the <paramref name="info" />.</param>
    /// <returns>A new <see cref="FileInfo" /> object.</returns>
    public static FileInfo MapToFileInfo(this FileSystemInfo info, string basePath)
    {
        return new FileInfo
        {
            Path = RemoveBeforeSubstring(info.FullName, basePath),
            Size = info is System.IO.FileInfo fileInfo ? fileInfo.Length : 0
        };
    }

    /// <summary>
    ///     Removes the portion of a string that occurs before (and including) the specified substring.
    ///     If the substring is not found, the original string is returned.
    /// </summary>
    /// <param name="input">The original string.</param>
    /// <param name="substring">The substring to find.</param>
    /// <returns>The part of the string after the substring, or the original string if the substring is not found.</returns>
    private static string RemoveBeforeSubstring(string input, string substring)
    {
        var index = input.IndexOf(substring, StringComparison.Ordinal);
        return index >= 0 ? input[(index + substring.Length)..] : input;
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