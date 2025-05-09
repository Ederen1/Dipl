namespace Dipl.Business.Entities;

/// <summary>
///     A link representing sent files to a user.
/// </summary>
public class UploadLink : BaseLink
{
    public DateTime? Uploaded { get; set; }
}