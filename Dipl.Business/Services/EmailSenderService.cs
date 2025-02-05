using System.Net.Mail;
using Dipl.Business.EmailModels;
using Dipl.Business.EmailTemplates;
using Dipl.Business.Entities;
using Dipl.Business.Models;
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
        var models = model.SendTo.Select(sendTo => new NotifyUserRequestedModel
        {
            Model = model,
            RequestLinkId = linkId,
            CurrentlySendingTo = sendTo
        });

        var renderedTemplates = await Task.WhenAll(models.Select(RenderTemplate<NotifyUserRequested>));
        var sender = await usersService.GetCurrentUser();

        var emails = renderedTemplates.Select((template, i) => new MailMessage
        {
            From = new MailAddress(sender.Email),
            To = { model.SendTo[i] },
            Subject = $"User {sender.UserName} is sending you files",
            Body = template,
            IsBodyHtml = true
        });

        foreach (var email in emails)
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
            UploadLinkId = link.LinkId
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

    public async Task NotifyUserUploadedToRequestLink(NotifyRequestUploadedModel model,
        CancellationToken cancellationToken)
    {
        var body = await RenderTemplate<NotifyUserUploadedRequest>(model);

        var email = new MailMessage
        {
            From = Sender,
            To = { string.Join(',', model.EmailTo) },
            Subject = $"User {model.ResponderEmail} uploaded files to {model.LinkTitle}",
            Body = body,
            IsBodyHtml = true
        };

        try
        {
            await smtpClient.SendMailAsync(email, cancellationToken);
        }
        catch
        {
            logger.LogError("Unable to send email to {receiver}", model.EmailTo);
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
}