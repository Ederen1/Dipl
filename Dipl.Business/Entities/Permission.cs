namespace Dipl.Business.Entities;

public class Permission
{
    public int PermissionId { get; set; }
    public required bool Read { get; set; }
    public required bool Write { get; set; }

    public int GroupId { get; set; }
    public virtual Group Group { get; set; } = null!;
    public virtual ICollection<User> Users { get; set; } = [];
    public const int GuestPermissionId = -1;
}