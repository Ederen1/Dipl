using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Entities;

public class Group
{
    [Key]
    public int GroupId { get; set; }
    public required string Name { get; set; }
    public virtual Permission Permission { get; set; } = null!;
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public const int GuestGrupId = -1;
}