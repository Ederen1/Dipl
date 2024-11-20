namespace Dipl.Business.Models;

public class NotifyUserRequestedModel
{
    public required Guid RequestLinkId { get; set; }

    public required RequestLinkModel Model { get; set; }
}