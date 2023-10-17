namespace Dipl.Business.Entities;

public class Link
{
    public string LinkId { get; set; } = null!;
    public required string Folder { get; set; }
    
    public virtual User User { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}