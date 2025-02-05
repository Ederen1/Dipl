using Dipl.Business.Models;

namespace Dipl.Business.EmailModels;

public class NotifyUserRequestedModel
{
    public required Guid RequestLinkId { get; set; }

    public required RequestLinkModel Model { get; set; }
    public required string CurrentlySendingTo { get; set; }
}