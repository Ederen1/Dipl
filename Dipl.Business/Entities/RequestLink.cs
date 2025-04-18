using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Entities;

public class RequestLink : BaseLink
{
    public bool NotifyOnUpload { get; set; }
    public bool IsProtected { get; set; }

    public string[] AllowedExtensions = [];

    public long AllFilesSizeLimit { get; set; } = long.MaxValue;
    public virtual ICollection<RequestLinkUploadSlot> UploadSlots { get; set; } = [];
}