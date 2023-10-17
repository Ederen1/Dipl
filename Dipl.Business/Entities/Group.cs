namespace Dipl.Business.Entities;

public class Group
{
    public int GroupId { get; set; }
    public virtual Permission Permission { get; set; } = null!;
}