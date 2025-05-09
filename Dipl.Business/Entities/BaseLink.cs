using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Entities;

/// <summary>
///     Common properties for both Upload and Request links.
/// </summary>
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

    // Properties for link protection using password-based key derivation.
    /// <summary>
    ///     Salt used for deriving the verifier hash.
    /// </summary>
    public byte[]? VerifierSalt { get; set; }

    /// <summary>
    ///     Hash used to verify the password, derived from VerifierSalt and password.
    /// </summary>
    public byte[]? VerifierHash { get; set; }

    /// <summary>
    ///     Salt used for encrypting/decrypting file content.
    /// </summary>
    public byte[]? Salt { get; set; }
}