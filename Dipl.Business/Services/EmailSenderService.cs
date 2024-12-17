using System.Net;
using System.Net.Mail;
using Dipl.Business.EmailTemplates;
using Dipl.Business.Entities;
using Dipl.Business.Models;
using Dipl.Business.Services.Interfaces;
using Dipl.Common.Configs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dipl.Business.Services;

public class EmailSenderService(
    IOptions<EmailSenderSettings> settings,
    ILogger<EmailSenderService> logger,
    SmtpClient smtpClient,
    HtmlRenderer htmlRenderer,
    UsersService usersService)
{
    private MailAddress Sender => new(settings.Value.SenderEmail);

    public async Task NotifyOfRequest(RequestLinkModel model, Guid linkId)
    {
        var newModel = new NotifyUserRequestedModel
        {
            Model = model,
            RequestLinkId = linkId
        };
        
        var body = await RenderTemplate<NotifyUserRequested>(newModel);
        var sender = await usersService.GetCurrentUser();

        var email = new MailMessage
        {
            From = Sender,
            To = { string.Join(',', model.SendTo) },
            Subject = $"User {sender.UserName} is sending you files",
            Body = body,
            IsBodyHtml = true
        };
        
        try
        {
            await smtpClient.SendMailAsync(email);
        }
        catch
        {
            logger.LogError("Unable to send email to {receiver}", string.Join(',', model.SendTo));
        }
    }

    public async Task NotifyUserUploaded(UploadLink link, CreateUploadLinkModel model)
    {
        var newModel = new NotifyUserUploadedModel
        {
            Model = model,
            UploadLinkId = link.UploadLinkId
        };
        
        var body = await RenderTemplate<NotifyUserUploaded>(newModel);

        var email = new MailMessage
        {
            From = Sender,
            To = { string.Join(',', model.EmailTo) },
            Subject = $"User {model.Sender} is sending you files",
            Body = body,
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

    private async Task<string> RenderTemplate<TElem>(object model) where TElem : ComponentBase
    {
        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var dictionary = new Dictionary<string, object?>
            {
                { "Model", model }
            };

            var parameters = ParameterView.FromDictionary(dictionary);
            var output = await htmlRenderer.RenderComponentAsync<TElem>(parameters);

            return output.ToHtmlString();
        });

        return html;
    }

//     public async Task NotifyUploadForUser(Link link, UploadLinkModel model, string? uploader)
//     {
//         var user = WebUtility.HtmlEncode(uploader ?? model.GuestEmail)!;
//         var listOfFiles = (await storeService.List(link.Folder)).Select(f => f.Name);
//
//         var message = WebUtility.HtmlEncode(model.MessageForUser);
//         var linkUrl = navigationManager.ToAbsoluteUri($"/link/{link.LinkId}");
//         var linkUrlEncoded = WebUtility.HtmlEncode(linkUrl.ToString());
//         var formattedBody = $"""
//                              <h1>Uploader: {user}</h1>
//                              <pre>{message}</pre>
//                              <b>Files uploaded:</b>
//                              <ul>
//                                  {string.Join(string.Empty, listOfFiles.Select(f => $"<li>{WebUtility.HtmlEncode(f)}</li>"))}
//                              </ul>
//                              <a href="{linkUrlEncoded}">Click here to download files</a>
//                              """;
//
//         var email = new MailMessage
//         {
//             From = Sender,
//             Subject = $"{uploader ?? model.GuestEmail} sent you files {WebUtility.HtmlEncode(link.LinkTitle)}",
//             Body = formattedBody,
//             IsBodyHtml = true
//         };
//
//         foreach (var send in model.EmailTo)
//             email.To.Add(send);
//
//         try
//         {
//             await smtpClient.SendMailAsync(email);
//         }
//         catch
//         {
//             logger.LogError("Unable to send email to {receiver}", link.CreatedBy.Email);
//         }
//     }
}