using Dipl.Business.Models;

namespace Dipl.Business.EmailModels;

public class NotifyRequestUploadedModel
{
    public required string LinkTitle { get; set; }
    public required string EmailTo { get; set; }
    public required string ResponderEmail { get; set; }
    public required string Message { get; set; }
    public required FileInfoModel[] Files { get; set; }
}