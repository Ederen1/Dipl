namespace Dipl.Business.Entities;

public class RequestLink : BaseLink
{
    public bool NotifyOnUpload { get; set; }
    public virtual ICollection<RequestLinkUploadSlot> UploadSlots { get; set; } = [];
}