namespace Dipl.Business.Models;

public class UploadFileModel
{
    public required string Name { get; set; }
    public long Size { get; set; }
    public int Progress { get; set; }
    public bool Started { get; set; }

    public required Func<long, CancellationToken, Stream> OpenReadStream;

    public bool Done => Progress >= 100;
}