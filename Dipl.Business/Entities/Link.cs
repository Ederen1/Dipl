using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dipl.Business.Entities;

public class Link
{
    public Guid LinkId { get; set; }
    [MaxLength(255)] public required string Folder { get; set; }
    [Column(TypeName = "INT")] public LinkTypeEnum LinkType { get; set; } = LinkTypeEnum.Upload;
    [MaxLength(10_000)] public string? Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastAccessed { get; set; } = DateTime.Now;
    public required Guid CreatedById { get; set; }
    public virtual User CreatedBy { get; set; } = null!;
    public virtual required Permission Permission { get; set; }
    public bool NotifyOnUpload { get; set; }
    public bool LinkClosed { get; set; }
    [MaxLength(200)]
    public string? LinkName { get; set; }
}