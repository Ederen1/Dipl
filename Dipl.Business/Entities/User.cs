namespace Dipl.Business.Entities;

public class User
{
    public required Guid UserId { get; set; }
    public virtual ICollection<Link> Links { get; set; } = new List<Link>();
}