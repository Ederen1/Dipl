namespace Dipl.Common.Configs;

public record EmailSenderSettings
{
    public required string SenderEmail { get; init; }
}