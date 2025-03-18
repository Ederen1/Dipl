namespace Dipl.Common.Configs;

public class FileStoreServiceConfiguration
{
    public required string BasePath { get; set; }
}

public class FTPFileStoreServiceConfiguration
{
    public required string Host { get; set; }
    public required int? Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string BasePath { get; set; }
}