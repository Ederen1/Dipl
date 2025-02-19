using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Entities;

public class UploadLink : BaseLink;

public abstract class BaseLink
{
    [Key]
    public Guid LinkId { get; set; }

    [MaxLength(200)]
    public string LinkTitle { get; set; } = null!;

    [MaxLength(10_000)]
    public string? Message { get; set; }

    public required string CreatedById { get; set; }
    public virtual User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastAccessed { get; set; } = DateTime.Now;
}