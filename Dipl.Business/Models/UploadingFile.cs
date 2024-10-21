namespace Dipl.Business.Models;

public class UploadFileModel
{
    public required Func<long, CancellationToken, Stream> OpenReadStream;
    public required string Name { get; set; }
    public long Size { get; set; }
    public int Progress { get; set; }
    public required CancellationTokenSource CancellationTokenSource { get; set; }

    public bool Done => Progress >= 100;
}