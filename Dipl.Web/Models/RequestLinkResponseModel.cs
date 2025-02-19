using Blazorise;
using Dipl.Business.EmailModels;
using Dipl.Business.Entities;
using Dipl.Business.Models;

namespace Dipl.Web.Models;

public class RequestLinkResponseModel
{
    public string Message { get; set; } = "";
    public List<IFileEntry> FilesToUpload { get; set; } = [];

    public NotifyRequestUploadedModel MapToNotifyRequestUploadedModel(RequestLink link, RequestLinkUploadSlot slot)
    {
        return new NotifyRequestUploadedModel
        {
            Message = Message,
            LinkTitle = link.LinkTitle!,
            Link = link,
            UploadSlot = slot,
            EmailTo = link.CreatedBy.Email,
            Files = FilesToUpload.Select(x => new FileInfoModel
            {
                Name = x.Name,
                Size = x.Size
            }).ToArray()
        };
    }
}