using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Entities;

public class User
{
    /// <summary>
    ///     Predefined User ID for guest users or anonymous operations.
    /// </summary>
    public static readonly string GuestUserId = "f55aa676-775d-4312-b31c-e9d5848e06d7";

    /// <summary>
    ///     Primary key, provided by the SSO system.
    /// </summary>
    public string UserId { get; set; } = null!;

    [MaxLength(256)]
    public required string Email { get; set; }

    [MaxLength(256)]
    public required string UserName { get; set; }

    /// <summary>
    ///     Collection of upload links created by this user.
    /// </summary>
    public virtual ICollection<UploadLink> UploadLinks { get; set; } = [];

    /// <summary>
    ///     Collection of request links created by this user.
    /// </summary>
    public virtual ICollection<RequestLink> RequestLinks { get; set; } = [];
}