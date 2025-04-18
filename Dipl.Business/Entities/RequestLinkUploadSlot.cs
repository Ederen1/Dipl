using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Entities;

public class RequestLinkUploadSlot
{
    public Guid RequestLinkUploadSlotId { get; set; }

    [MaxLength(100)]
    public required string Email { get; set; }

    [MaxLength(10_000)]
    public string Message { get; set; } = "";

    public DateTime? Uploaded { get; set; }

    public Guid RequestLinkId { get; set; }
    public bool IsProtected { get; set; }
    public virtual RequestLink RequestLink { get; set; } = null!;
}