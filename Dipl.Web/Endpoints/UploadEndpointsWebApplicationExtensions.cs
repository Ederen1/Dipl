using Blazorise;
using Dipl.Business;
using Dipl.Business.Entities;
using Dipl.Business.Services;
using Dipl.Business.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dipl.Web.Endpoints;

public static class UploadEndpointsWebApplicationExtensions
{
    public static void MapUploadEncpoints(this WebApplication app)
    {
        app.MapPost("/uploadFiles", async (HttpRequest req, AppDbContext dbContext, IStoreService storeService, CancellationToken cancellationToken) =>
        {
            if (!req.HasFormContentType)
                return Results.BadRequest();

            var form = await req.ReadFormAsync(cancellationToken);
            var linkId = form["linkId"];
            var slotId = form["slotId"];
            var password = form["password"];

            string folder;
            BaseLink baseLink;
            if (string.IsNullOrEmpty(slotId))
            {
                var link = await dbContext.UploadLinks.FirstOrDefaultAsync(x => x.LinkId == Guid.Parse(linkId!), cancellationToken: cancellationToken);
                if (link is null)
                    return Results.NotFound("Link not found");

                folder = link.LinkId.ToString();
                baseLink = link;
            }
            else
            {
                var link = await dbContext.RequestLinks.FirstOrDefaultAsync(x => x.LinkId == Guid.Parse(linkId!), cancellationToken: cancellationToken);
                if (link is null)
                    return Results.NotFound("Link not found");

                var slot = link.UploadSlots.FirstOrDefault(x => x.RequestLinkUploadSlotId == slotId);
                if (slot is null)
                    return Results.NotFound("Slot not found");

                folder = $"{link.LinkId}/{slot.RequestLinkUploadSlotId}";
                baseLink = link;
            }
            
            if (!string.IsNullOrWhiteSpace(password))
            {
                if (!await LinkSecurityService.PasswordMatchesLink(baseLink, password))
                    return Results.Unauthorized();
            }

            if (form.Files.Count == 0)
                return Results.BadRequest();
            
            foreach (var file in form.Files)
            {
                await using var stream = file.OpenReadStream();
                if (baseLink.Salt is not null)
                {
                    await using var cryptStream = await LinkSecurityService.EncryptDataAsync(baseLink, password!, stream);
                    await storeService.InsertFile(file.FileName, folder, cryptStream);
                }
                else
                {
                    await storeService.InsertFile(file.FileName, folder, stream);
                }

            }
            
            
            return Results.Ok();   
        });
    }
}