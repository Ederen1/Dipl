using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Entities;

public class Group
{
    public int GroupId { get; set; }
    [MaxLength(256)] public required string Name { get; set; }
    [MaxLength(1024)] public string Description { get; set; } = "";
    public virtual Permission Permission { get; set; } = null!;
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public const int GuestGrupId = -1;
}