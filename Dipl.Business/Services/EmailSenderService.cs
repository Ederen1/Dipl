using System.Net;
using System.Net.Mail;
using Dipl.Business.Entities;
using Dipl.Business.Models;
using Dipl.Business.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dipl.Business.Services;

public class EmailSenderService(IConfiguration configuration, ILogger<EmailSenderService> logger, SmtpClient smtpClient, IStoreService storeService)
{
    private readonly string _domain = configuration["Domain"]!;
    private readonly string _notifySender = configuration["NotifySenderName"]!;
    private MailAddress Sender => new($"{_notifySender}@{_domain}");

    public async Task NotifyOfRequest(RequestLinkModel request, string requestingUserName, string linkUrl)
    {
        var requestingUsernameEncoded = WebUtility.HtmlEncode(requestingUserName);
        var linkNameEncoded = WebUtility.HtmlEncode(request.LinkName);
        // TODO: When we support html in message, we have to sanitize it in a different way
        var messageForUserEncoded = WebUtility.HtmlEncode(request.MessageForUser);
        var linkUrlEncoded = WebUtility.HtmlEncode(linkUrl);
        
        var formattedBody = $"""
                             <h1>User: {requestingUsernameEncoded}</h1>
                             <p>Is requesting files for <b>{linkNameEncoded}</b></p>
                             <br>
                             <p>Message from user:</p>
                             <p>{messageForUserEncoded}</p>

                             <a href="{linkUrlEncoded}">Click here to upload files</a>
                             """;
        var mail = new MailMessage
        {
            From = Sender,
            Subject = $"Request for files from {requestingUserName}",
            Body = formattedBody,
            IsBodyHtml = true,
        };

        foreach (var receiver in request.SendTo)
            mail.To.Add(receiver);

        try
        {
            await smtpClient.SendMailAsync(mail);
        }
        catch
        {
            logger.LogError("Unable to send email to {receivers}", request.SendToSeparatedByCommas);
        }
    }

    public async Task NotifyUserUploaded(Link link, string? uploader)
    {
        var userMessage = "User";
        if (uploader != null)
            userMessage += $" {uploader}";

        userMessage = WebUtility.HtmlEncode(userMessage);
        
        var listOfFiles = (await storeService.List(link.Folder)).Select(f => f.Name);

        var uploaderMessage = uploader != null ? $"<h2>Uploader: {uploader}</h2>" : "";
        uploaderMessage = WebUtility.HtmlEncode(uploaderMessage);
        var formattedBody = $"""
                             {uploaderMessage}
                             <b>Files uploaded:</b>
                             <ul>
                                 {string.Join(string.Empty, listOfFiles.Select(f => $"<li>{WebUtility.HtmlEncode(f)}</li>"))}
                             </ul>
                             """;
        
        var email = new MailMessage
        {
            From = Sender,
            To = { link.CreatedBy.Email },
            Subject = $"{userMessage} uploaded files to {WebUtility.HtmlEncode(link.Folder.Split("/").Last())}",
            Body = formattedBody,
            IsBodyHtml = true,
        };
        try
        {
            await smtpClient.SendMailAsync(email);
        }
        catch
        {
            logger.LogError("Unable to send email to {receiver}", link.CreatedBy.Email);
        }
    }
}