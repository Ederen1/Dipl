namespace Dipl.Business.Models;

public class RequestLinkCreateModel
{
    public string LinkName { get; set; } = null!;
    public string MessageForUser { get; set; } = "";
    public List<string> SendTo { get; set; } = [];
    public bool NotifyOnUpload { get; set; }
    public bool IsProtected { get; set; }
    public long? SizeLimitPremultiplied { get; set; }
    public long SizeLimitMultiplier { get; set; } = 1048576; // MB
    public string[] AllowedExtensions { get; set; } = [];
}