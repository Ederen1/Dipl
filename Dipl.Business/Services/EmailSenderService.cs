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

/// <summary>
///     Service used for sending emails for the whole web app.
/// </summary>
public class EmailSenderService(
    IOptions<EmailSenderSettings> settings,
    ILogger<EmailSenderService> logger,
    SmtpClient smtpClient,
    HtmlRenderer htmlRenderer,
    UsersService usersService)
{
    private MailAddress Sender => new(settings.Value.SenderEmail);

    /// <summary>
    ///     Sends email notifications to users specified in a new file request.
    /// </summary>
    public async Task NotifyOfRequest(RequestLinkCreateModel createModel, RequestLink link, string senderName)
    {
        var models = link.UploadSlots.Select(slot => new NotifyUserRequestedModel
        {
            CreateModel = createModel,
            RequestLinkId = link.LinkId,
            CurrentlySendingTo = slot.Email,
            SenderName = senderName,
            SlotId = slot.RequestLinkUploadSlotId
        });

        var renderedTemplates = await Task.WhenAll(models.Select(RenderTemplate<NotifyUserRequested>));
        var currentUser = await usersService.GetCurrentUser();

        var emails = renderedTemplates.Select((template, i) => new MailMessage
        {
            From = Sender,
            To = { createModel.SendTo[i] },
            Subject = $"{currentUser.UserName} is requesting files from you",
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
                logger.LogError("Unable to send email to {receiver}", string.Join(',', createModel.SendTo));
            }
    }

    /// <summary>
    ///     Sends an email notification to recipients when files are uploaded via an upload link.
    /// </summary>
    public async Task NotifyUserUploaded(UploadLink link, CreateUploadLinkModel model,
        CancellationToken cancellationToken = default)
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
            await smtpClient.SendMailAsync(email, cancellationToken);
        }
        catch
        {
            logger.LogError("Unable to send email to {receiver}", link.CreatedBy.Email);
        }
    }

    /// <summary>
    ///     Sends an email notification to the creator of a request link when files are uploaded to one of its slots.
    /// </summary>
    public async Task NotifyUserUploadedToRequestLink(NotifyRequestUploadedModel model,
        CancellationToken cancellationToken)
    {
        var body = await RenderTemplate<NotifyUserUploadedRequest>(model);

        var email = new MailMessage
        {
            From = Sender,
            To = { model.EmailTo },
            Subject = $"User {model.UploadSlot.Email} uploaded files to {model.LinkTitle}",
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

    /// <summary>
    ///     Renders a Razor component to an HTML string.
    /// </summary>
    /// <typeparam name="TElem">The type of the Razor component to render.</typeparam>
    /// <param name="model">The model to pass to the Razor component.</param>
    /// <returns>The rendered HTML string.</returns>
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