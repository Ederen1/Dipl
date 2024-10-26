using System.Net;
using System.Net.Mail;
using Dipl.Business.Entities;
using Dipl.Business.Models;
using Dipl.Business.Services.Interfaces;
using Dipl.Common.Configs;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dipl.Business.Services;

public class EmailSenderService(
    IOptions<EmailSenderSettings> settings,
    ILogger<EmailSenderService> logger,
    SmtpClient smtpClient,
    IStoreService storeService,
    NavigationManager navigationManager
)
{
    private MailAddress Sender => new(settings.Value.SenderEmail);

    public async Task NotifyOfRequest(
        RequestLinkModel request,
        string requestingUserName,
        string linkUrl
    )
    {
        if (request.SendTo.Length == 0)
            return;

        var requestingUsernameEncoded = WebUtility.HtmlEncode(requestingUserName);
        var linkNameEncoded = WebUtility.HtmlEncode(request.LinkName);
        // TODO: When we support html in message, we have to sanitize it in a different way
        var messageForUserEncoded = WebUtility.HtmlEncode(request.MessageForUser);
        var linkUrlEncoded = WebUtility.HtmlEncode(linkUrl);

        var formattedBody = $"""
                             <h1>User: {requestingUsernameEncoded}</h1>
                             <p>Is requesting files for <b>{linkNameEncoded}</b></p>
                             <p>Message from user:</p>
                             <p>{messageForUserEncoded}</p>

                             <a href="{linkUrlEncoded}">Click here to upload files</a>
                             """;
        var mail = new MailMessage
        {
            From = Sender,
            Subject = $"Request for files from {requestingUserName}",
            Body = formattedBody,
            IsBodyHtml = true
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

        var listOfFiles = (await storeService.List(link.Folder)).Select(f => f.Name);

        var uploaderMessage =
            uploader != null ? $"<h2>Uploader: {WebUtility.HtmlEncode(uploader)}</h2>" : "";
        var formattedBody = $"""
                             {uploaderMessage}
                             <b>Files uploaded:</b>
                             <ul>
                                 {string.Join(string.Empty, listOfFiles.Select(f => $"<li>{WebUtility.HtmlEncode(f)}</li>"))}
                             </ul>
                             <a href="{navigationManager.ToAbsoluteUri($"/download/{link.LinkId}")}">Download files</a>
                             """;

        var email = new MailMessage
        {
            From = Sender,
            To = { link.CreatedBy.Email },
            Subject =
                $"User {userMessage} uploaded files to {WebUtility.HtmlEncode(link.LinkName)}",
            Body = formattedBody,
            IsBodyHtml = true
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

    public async Task NotifyUploadForUser(Link link, UploadLinkModel model, string? uploader)
    {
        var user = WebUtility.HtmlEncode(uploader ?? model.Sender)!;
        var listOfFiles = (await storeService.List(link.Folder)).Select(f => f.Name);

        var message = WebUtility.HtmlEncode(model.MessageForUser);
        var linkUrl = navigationManager.ToAbsoluteUri($"/link/{link.LinkId}");
        var linkUrlEncoded = WebUtility.HtmlEncode(linkUrl.ToString());
        var formattedBody = $"""
                             <h1>Uploader: {user}</h1>
                             <pre>{message}</pre>
                             <b>Files uploaded:</b>
                             <ul>
                                 {string.Join(string.Empty, listOfFiles.Select(f => $"<li>{WebUtility.HtmlEncode(f)}</li>"))}
                             </ul>
                             <a href="{linkUrlEncoded}">Click here to download files</a>
                             """;

        var email = new MailMessage
        {
            From = Sender,
            Subject =
                $"{uploader ?? model.Sender} sent you files {WebUtility.HtmlEncode(link.LinkName)}",
            Body = formattedBody,
            IsBodyHtml = true
        };

        foreach (var send in model.SendTo)
            email.To.Add(send);

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