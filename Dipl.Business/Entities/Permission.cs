using System.ComponentModel.DataAnnotations.Schema;

namespace Dipl.Business.Entities;

public class Permission
{
    public int PermissionId { get; set; }
    public required bool Read { get; set; }
    public required bool Write { get; set; }

    [ForeignKey(nameof(Group))]
    public int GroupId { get; set; }
    public virtual Group Group { get; set; } = null!;
    public const int GuestPermissionId = -1;
}