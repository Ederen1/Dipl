using System.Globalization;

namespace Dipl.Common.Util;

public static class FileUtils
{
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
    
    public static string SanitizePath(string path)
    {
        var notAllowed = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars());

        path = path.Replace(' ', '_');
        path = path.ReplaceLineEndings("_");
        path = new string(path.Where(c => !notAllowed.Contains(c)).ToArray());
        return path;
    }
}