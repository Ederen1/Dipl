using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Entities;

public class RequestLinkUploadSlot
{
    public Guid RequestLinkUploadSlotId { get; set; }
    [MaxLength(100)]
    public required string Email { get; set; }
    public required bool Closed { get; set; }
    
    public Guid RequestLinkId { get; set; }
    public virtual RequestLink RequestLink { get; set; }
}