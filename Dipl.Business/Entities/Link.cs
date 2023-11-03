namespace Dipl.Business.Entities;

public class Link
{
    public Guid LinkId { get; set; }
    public required string Folder { get; set; }
    public virtual required User CreatedBy { get; set; }
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
}