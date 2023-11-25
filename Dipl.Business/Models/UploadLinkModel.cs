using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Models;

public class UploadLinkModel : RequestLinkModel
{
    public Guid LinkId { get; set; } = Guid.NewGuid();
    
    [MaxLength(100)]
    public string? Sender { get; set; }
};