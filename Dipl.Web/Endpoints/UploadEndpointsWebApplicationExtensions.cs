using Dipl.Business;
using Dipl.Business.Entities;
using Dipl.Business.Services;
using Dipl.Business.Services.Interfaces;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace Dipl.Web.Endpoints;

public static class UploadEndpointsWebApplicationExtensions
{
    private static readonly FormOptions _defaultFormOptions = new();

    public static void MapUploadEncpoints(this WebApplication app)
    {
        app.MapPost("/uploadFiles", async (HttpRequest req, AppDbContext dbContext, IStoreService storeService,
            CancellationToken cancellationToken) =>
        {
            if (!MultipartRequestHelper.IsMultipartContentType(req.ContentType))
                return Results.BadRequest("Not a multipart request");

            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(req.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);

            var reader = new MultipartReader(boundary, req.Body, 81 * 1024);
            ;
            var section = await reader.ReadNextSectionAsync(cancellationToken);

            string? linkId = null;
            string? slotId = null;
            string? password = null;

            // Read form data first
            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader && contentDisposition!.DispositionType.Equals("form-data"))
                {
                    if (contentDisposition.FileName.HasValue)
                        break;
                    if (contentDisposition.Name.Equals("linkId"))
                        linkId = await new StreamReader(section.Body).ReadToEndAsync(cancellationToken);
                    else if (contentDisposition.Name.Equals("slotId"))
                        slotId = await new StreamReader(section.Body).ReadToEndAsync(cancellationToken);
                    else if (contentDisposition.Name.Equals("password"))
                        password = await new StreamReader(section.Body).ReadToEndAsync(cancellationToken);
                }

                section = await reader.ReadNextSectionAsync(cancellationToken);
            }

            if (string.IsNullOrEmpty(linkId))
                return Results.BadRequest("LinkId is required");

            string folder;
            BaseLink baseLink;
            if (string.IsNullOrEmpty(slotId))
            {
                var link = await dbContext.UploadLinks.FirstOrDefaultAsync(x => x.LinkId == Guid.Parse(linkId!),
                    cancellationToken);
                if (link is null)
                    return Results.NotFound("Link not found");

                folder = link.LinkId.ToString();
                baseLink = link;
            }
            else
            {
                var link = await dbContext.RequestLinks.FirstOrDefaultAsync(x => x.LinkId == Guid.Parse(linkId!),
                    cancellationToken);
                if (link is null)
                    return Results.NotFound("Link not found");

                var slot = link.UploadSlots.FirstOrDefault(x => x.RequestLinkUploadSlotId == Guid.Parse(slotId!));
                if (slot is null)
                    return Results.NotFound("Slot not found");

                folder = $"{link.LinkId}/{slot.RequestLinkUploadSlotId}";
                baseLink = link;
            }

            if (!string.IsNullOrWhiteSpace(password))
                if (!await LinkSecurityService.PasswordMatchesLink(baseLink, password))
                    return Results.Unauthorized();


            var filesProcessed = 0;
            while (section != null)
            {
                var contentDispositionHeader = section.GetContentDispositionHeader();

                if (contentDispositionHeader is not null &&
                    contentDispositionHeader!.DispositionType.Equals("form-data") &&
                    contentDispositionHeader.IsFileDisposition())
                {
                    var fileSection = section.AsFileSection();

                    if (baseLink.VerifierHash is not null)
                    {
                        await using var cryptStream =
                            await LinkSecurityService.EncryptDataAsync(baseLink, password!, fileSection!.FileStream!);
                        await storeService.InsertFile(fileSection!.FileName, folder, cryptStream);
                    }
                    else
                    {
                        await storeService.InsertFile(fileSection!.FileName, folder, fileSection!.FileStream!);
                    }

                    filesProcessed++;
                }

                section = await reader.ReadNextSectionAsync(cancellationToken);
            }

            if (filesProcessed == 0)
                return Results.BadRequest("No files were uploaded");

            await dbContext.SaveChangesAsync(cancellationToken);
            return Results.Ok();
        });
    }
}

internal static class MultipartRequestHelper
{
    public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;
        if (string.IsNullOrWhiteSpace(boundary))
            throw new InvalidDataException("Missing content-type boundary.");

        if (boundary.Length > lengthLimit)
            throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");

        return boundary;
    }

    public static bool IsMultipartContentType(string? contentType)
    {
        return !string.IsNullOrEmpty(contentType) &&
               contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);
    }
}