using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dipl.Business.Entities;

public class UploadLink : BaseLink
{
    public Guid UploadLinkId { get; set; }
    public override Guid LinkId => UploadLinkId;
}

public abstract class BaseLink
{
    [MaxLength(200)]
    public string? LinkTitle { get; set; }
    [MaxLength(255)]
    public required string Folder { get; set; }
    [MaxLength(10_000)]
    public string? Message { get; set; }
    public bool LinkClosed { get; set; }

    public required string CreatedById { get; set; }
    public virtual User CreatedBy { get; set; } = null!;
    
    public int PermissionId { get; set; }
    public virtual required Permission Permission { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastAccessed { get; set; } = DateTime.Now;
    public abstract Guid LinkId { get; }
}