namespace Dipl.Business.Entities;

/// <summary>
///     Link for requesting files from user(s).
/// </summary>
public class RequestLink : BaseLink
{
    /// <summary>
    ///     Specifies allowed file extensions for uploads (e.g., ".pdf", ".docx"). An empty array means all extensions are
    ///     allowed.
    /// </summary>
    public string[] AllowedExtensions = [];

    /// <summary>
    ///     If true, the creator of the link will be notified upon file upload.
    /// </summary>
    public bool NotifyOnUpload { get; set; }

    /// <summary>
    ///     If true, the uploader must confirm their email address matches the one the request was sent to.
    /// </summary>
    public bool IsProtected { get; set; }

    /// <summary>
    ///     Maximum allowed total size for all files in a single upload slot. MaxValue means no limit.
    /// </summary>
    public long AllFilesSizeLimit { get; set; } = long.MaxValue;

    public virtual ICollection<RequestLinkUploadSlot> UploadSlots { get; set; } = [];
}