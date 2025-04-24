using Blazorise;
using Dipl.Business.Models;
using Dipl.Common.Util;

namespace Dipl.Web.Models;

public class FileUploadModel
{
    public string? GuestEmail { get; set; }
    public List<string> EmailTo { get; set; } = [];
    public string LinkTitle { get; set; } = null!;
    public string MessageForUser { get; set; } = "";
    public List<IFileEntry> FilesToUpload { get; set; } = [];
    public string? Password;
    public string? MatchingPassword;

    public CreateUploadLinkModel MapToCreateUploadModel(string? userName)
    {
        return new CreateUploadLinkModel
        {
            Sender = userName ?? GuestEmail!,
            FullFolderName = Path.Combine(userName ?? GuestEmail!, FileUtils.SanitizePath(LinkTitle)),
            LinkTitle = LinkTitle,
            MessageForUser = MessageForUser,
            EmailTo = EmailTo,
            UploadedFileInfoModels = FilesToUpload.Select(f => new FileInfoModel
            {
                Name = f.Name,
                Size = f.Size
            }).ToList()
        };
    }
}