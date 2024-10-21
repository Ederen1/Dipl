namespace Dipl.Common.Types;

public class FileInfo
{
    public DateTime Created;
    public bool IsFolder;
    public required string Path;
    public long Size;
    public DateTime Updated;
    public string Name => System.IO.Path.GetFileName(Path);
}