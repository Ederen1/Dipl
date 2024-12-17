using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dipl.Business.Entities;

public class RequestLink : BaseLink
{
    public Guid RequestLinkId { get; set; }
    public bool NotifyOnUpload { get; set; }
    public override Guid LinkId => RequestLinkId;
}