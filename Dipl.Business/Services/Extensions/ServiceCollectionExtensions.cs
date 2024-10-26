using System.Net;
using System.Net.Mail;
using Dipl.Business.Services.Interfaces;
using Dipl.Common.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dipl.Business.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServiceLayer(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<FileManagerService>();
        serviceCollection.AddScoped<LinksService>();
        serviceCollection.AddScoped<UsersService>();
        serviceCollection.AddScoped<EmailSenderService>();
        serviceCollection.AddScoped<SmtpClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<SmtpSettings>>().Value;
            var smtpClient = new SmtpClient
            {
                Host = settings.Host,
                Port = settings.Port,
                Credentials = new NetworkCredential(settings.UserName, settings.Password),
                EnableSsl = settings.EnableSsl,
                UseDefaultCredentials = false
            };

            return smtpClient;
        });

        serviceCollection.AddScoped<IStoreService, FileStoreService>();
    }
}