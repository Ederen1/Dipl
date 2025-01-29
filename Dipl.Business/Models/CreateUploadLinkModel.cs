namespace Dipl.Business.Models;

public class CreateUploadLinkModel
{
    public required string Sender { get; set; }
    public required string LinkTitle { get; set; }
    public string? MessageForUser { get; set; }
    public required string FullFolderName { get; set; }
    public List<string> EmailTo { get; set; } = [];
    public List<FileInfoModel> UploadedFileInfoModels { get; set; } = [];
}