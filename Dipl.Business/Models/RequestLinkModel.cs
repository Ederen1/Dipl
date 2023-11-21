using System.ComponentModel.DataAnnotations;

namespace Dipl.Business.Models;

public class RequestLinkModel
{
    public string[] SendTo { get; private set; } = [];
    
    [Display(Name = "Send to")]
    public string SendToSeparatedByCommas
    {
        set => SendTo = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.TrimEntries);
        get => string.Join(", ", SendTo);
    }
    
    [Required]
    [Display(Name = "Link name")]
    public string LinkName { get; set; } = null!;
    
    [Display(Name = "Message for user")]
    public string MessageForUser { get; set; } = string.Empty;
}