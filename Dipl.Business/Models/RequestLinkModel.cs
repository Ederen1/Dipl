using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Models;

public class RequestLinkModel
{
    public string[] SendTo { get; private set; } = [];
    
    [Display(Name = "Send to")]
    [MaxLength(1000)]
    [RegularExpression(@"^[\W]*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]+[\W]*,{1}[\W]*)*([\w+\-.%]+@[\w\-.]+\.[A-Za-z]+)[\W]*$", ErrorMessage = "Invalid email address(es)")]
    public string SendToSeparatedByCommas
    {
        set => SendTo = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.TrimEntries);
        get => string.Join(", ", SendTo);
    }
    
    [Required]
    [Display(Name = "Link name")]
    [MaxLength(200)]
    public string LinkName { get; set; } = null!;
    
    [MaxLength(10_000)]
    [Display(Name = "Message for user")]
    public string MessageForUser { get; set; } = string.Empty;

    public bool NotifyOnUpload { get; set; }
}