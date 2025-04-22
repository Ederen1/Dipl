namespace Dipl.Common.Types;

public class FileInfo
{
    public required string Path;
    public long Size;
    public string Name => System.IO.Path.GetFileName(Path);
}