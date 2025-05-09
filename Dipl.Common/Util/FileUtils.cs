using System.Globalization;

namespace Dipl.Common.Util;

/// <summary>
///     Utility methods for file operations.
/// </summary>
public static class FileUtils
{
    /// <summary>
    ///     Converts a byte count to a human-readable string representation (e.g., 1024 bytes to "1.0KB").
    /// </summary>
    /// <param name="byteCount">The number of bytes.</param>
    /// <returns>A human-readable string representing the byte count.</returns>
    public static string BytesToString(long byteCount)
    {
        string[] suf = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
        if (byteCount == 0)
            return "0" + suf[0];

        var bytes = Math.Abs(byteCount);
        var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        var num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture) + suf[place];
    }

    /// <summary>
    ///     Sanitizes a path string by replacing spaces and line endings with underscores
    ///     and removing invalid path and file name characters.
    /// </summary>
    /// <param name="path">The path string to sanitize.</param>
    /// <returns>A sanitized path string.</returns>
    public static string SanitizePath(string path)
    {
        var notAllowed = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars());

        path = path.Replace(' ', '_');
        path = path.ReplaceLineEndings("_");
        path = new string(path.Where(c => !notAllowed.Contains(c)).ToArray());
        return path;
    }
}