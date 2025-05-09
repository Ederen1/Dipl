using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Entities;

/// <summary>
///     Slot is one recipient for a file request link.
///     It's connected to a specific email address from which files are requested.
/// </summary>
public class RequestLinkUploadSlot
{
    public Guid RequestLinkUploadSlotId { get; set; }

    [MaxLength(100)]
    public required string Email { get; set; }

    [MaxLength(10_000)]
    public string Message { get; set; } = "";

    /// <summary>
    ///     Timestamp when files were uploaded to this slot. Null means no upload.
    /// </summary>
    public DateTime? Uploaded { get; set; }

    public Guid RequestLinkId { get; set; }
    public virtual RequestLink RequestLink { get; set; } = null!;
}