namespace Dipl.Common.Configs;

public class SmtpSettings
{
    public required string Host { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public bool EnableSsl { get; init; }
    public int Port { get; init; } = 25;
}