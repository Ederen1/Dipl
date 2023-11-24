namespace Dipl.Business.Models;

public class UploadLinkModel : RequestLinkModel
{
    public Guid LinkId { get; set; } = Guid.NewGuid();
};