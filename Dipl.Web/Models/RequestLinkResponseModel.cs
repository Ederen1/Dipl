using Blazorise;
using Dipl.Business.EmailModels;
using Dipl.Business.Models;

namespace Dipl.Web.Models;

public class RequestLinkResponseModel
{
    public string Message { get; set; } = "";
    public List<IFileEntry> FilesToUpload { get; set; } = [];
    public Guid LinkId { get; set; }
    public string ResponderEmail { get; set; } = "";

    public NotifyRequestUploadedModel MapToNotifyRequestUploadedModel(string? linkTitle, string emailTo) => new()
    {
        Message = Message,
        ResponderEmail = ResponderEmail,
        LinkTitle = linkTitle ?? "",
        EmailTo = emailTo,
        Files = FilesToUpload.Select(x => new FileInfoModel
        {
            Name = x.Name,
            Size = x.Size
        }).ToArray()
    };
}