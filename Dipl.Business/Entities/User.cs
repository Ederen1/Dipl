using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Entities;

public class User
{
    public Guid UserId { get; set; }
    [MaxLength(256)] public required string Email { get; set; }
    [MaxLength(256)] public required string UserName { get; set; }
    public virtual ICollection<Link> Links { get; set; } = new List<Link>();
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
    public static readonly Guid GuestUserId = Guid.Parse("f55aa676-775d-4312-b31c-e9d5848e06d7");
    
    
    public virtual ICollection<Permission> PermissionsAssociatedWithThisUser { get; set; } = [];
}