using Blazorise;
using Dipl.Business.Models;

namespace Dipl.Web.Models;

public class FileUploadModel
{
    public string? GuestEmail { get; set; }
    public List<string> EmailTo { get; set; } = [];
    public string LinkTitle { get; set; } = null!;
    public string? MessageForUser { get; set; }
    public List<IFileEntry> FilesToUpload { get; set; } = [];

    public CreateUploadLinkModel MapToCreateUploadModel(string? userName) => new CreateUploadLinkModel
    {
        Sender = userName ?? GuestEmail!,
        FullFolderName = Path.Combine(userName ?? GuestEmail!, SanitizePath(LinkTitle)),
        LinkTitle = LinkTitle,
        MessageForUser = MessageForUser,
        EmailTo = EmailTo,
        UploadedFileInfoModels = FilesToUpload.Select(f => new FileInfoModel
        {
            Name = f.Name,
            Size = f.Size,
        }).ToList()
    };
    
    private static string SanitizePath(string path)
    {
        var notAllowed = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars());

        path = path.Replace(' ', '_');
        path = path.ReplaceLineEndings("_");
        path = new string(path.Where(c => !notAllowed.Contains(c)).ToArray());
        return path;
    }
}