namespace Dipl.Business.Models;

public class NotifyUserUploadedModel
{
    public required Guid UploadLinkId { get; set; }

    public required CreateUploadLinkModel Model { get; set; }
}