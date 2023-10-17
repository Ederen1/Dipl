namespace Dipl.Business.Entities;

public class Permission
{
    public int PermissionId { get; set; }
    public required bool Read { get; set; }
    public required bool Write { get; set; }

    public int GroupId { get; set; }
    public virtual required Group Group { get; set; }
}