namespace Dipl.Business.Models;

public class RequestLinkCreateModel
{
    public string LinkName { get; set; } = null!;
    public string MessageForUser { get; set; } = "";
    public List<string> SendTo { get; set; } = [];
    public bool NotifyOnUpload { get; set; }
}