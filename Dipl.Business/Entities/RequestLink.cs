namespace Dipl.Business.Entities;

public class RequestLink : BaseLink
{
    public string[] AllowedExtensions = [];
    public bool NotifyOnUpload { get; set; }
    public bool IsProtected { get; set; }

    public long AllFilesSizeLimit { get; set; } = long.MaxValue;
    public virtual ICollection<RequestLinkUploadSlot> UploadSlots { get; set; } = [];
}