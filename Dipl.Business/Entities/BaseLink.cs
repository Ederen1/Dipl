using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Entities;

public abstract class BaseLink
{
    [Key]
    public Guid LinkId { get; set; }

    [MaxLength(200)]
    public string LinkTitle { get; set; } = null!;

    [MaxLength(10_000)]
    public string Message { get; set; } = "";

    public string CreatedById { get; set; }
    public virtual User CreatedBy { get; set; } = null!;

    public DateTime Created { get; set; } = DateTime.Now;
    
    public byte[]? Salt { get; set; }
    public byte[]? VerifierHash { get; set; }
    public byte[]? AesIV { get; set; }
}