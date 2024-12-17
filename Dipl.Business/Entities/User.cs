using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Entities;

public class User
{
    public string UserId { get; set; }

    [MaxLength(256)]
    public required string Email { get; set; }

    [MaxLength(256)]
    public required string UserName { get; set; }

    public virtual ICollection<UploadLink> UploadLinks { get; set; } = [];
    public virtual ICollection<RequestLink> RequestLinks { get; set; } = [];
    public virtual ICollection<Group> Groups { get; set; } = [];
    public virtual ICollection<Permission> PermissionsAssociatedWithThisUser { get; set; } = [];
    
    public static readonly string GuestUserId = "f55aa676-775d-4312-b31c-e9d5848e06d7";
}