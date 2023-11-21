using System.Net;
using System.Net.Mail;
using Dipl.Business.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dipl.Business.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServiceLayer(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<FileManagerService>();
        serviceCollection.AddScoped<LinksService>();
        serviceCollection.AddScoped<InitializationService>();
        serviceCollection.AddScoped<UsersService>();
        serviceCollection.AddScoped<EmailSenderService>();
        serviceCollection.AddScoped<SmtpClient>(sp =>
        {
            var configuration = sp.GetService<IConfiguration>() ?? throw new Exception("Configuration not found");
            var smtpHost = configuration["SMTPHost"]!;
            var smtpPort = int.Parse(configuration["SMTPPort"] ?? "25");
            var smtpUsername = configuration["SMTPUsername"];
            var smtpPassword = configuration["SMTPPassword"];
            var smtpUseSsl = bool.Parse(configuration["SMTPUseSSL"] ?? "true");

            var smtpClient = new SmtpClient
            {
                Host = smtpHost,
                Port = smtpPort,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = smtpUseSsl
            };
            
            return smtpClient;
        });

        serviceCollection.AddScoped<IStoreService, FileStoreService>(_ => new FileStoreService("folder/"));
    }
}