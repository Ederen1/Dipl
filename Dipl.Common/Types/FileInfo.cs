namespace Dipl.Common.Types;

public class FileInfo
{
    public required string Path;
    public DateTime Created;
    public DateTime Updated;
    public bool IsFolder;
    public long Size;
    public string Name => System.IO.Path.GetFileName(Path);
}
