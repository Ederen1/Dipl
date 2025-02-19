using System.IO.Compression;
using Dipl.Business;
using Dipl.Business.Services;
using Dipl.Business.Services.Interfaces;
using Dipl.Common.Util;
using Microsoft.EntityFrameworkCore;

namespace Dipl.Web.Endpoints;

public static class DownloadEndpointsWebApplicationExtensions
{
    public static void MapDownloadEndpoints(this WebApplication app)
    {
        app.MapGet("/download/fromUpload/{linkId:guid}",
            async (Guid linkId, AppDbContext dbContext, IStoreService storeService, HttpContext context) =>
            {
                var link = await dbContext.UploadLinks.FindAsync(linkId) ??
                           throw new Exception($"Unable to find link with id {linkId}");
                var files = await storeService.ListFolder(link.LinkId.ToString());
                var sanitizedTitle = FileUtils.SanitizePath(link.LinkTitle!);

                // Set the Content-Disposition header for the file name
                context.Response.Headers.ContentDisposition = $"attachment; filename=\"{sanitizedTitle}.zip\";";
                // Set the content type for ZIP files
                context.Response.Headers.ContentType = "application/zip";

                using var archive = new ZipArchive(context.Response.BodyWriter.AsStream(), ZipArchiveMode.Create, true);
                foreach (var file in files)
                {
                    await using var fs = await storeService.GetFile(file.Path);
                    var entry = archive.CreateEntry(file.Name, CompressionLevel.Optimal);
                    await using var entryStream = entry.Open();
                    await fs.CopyToAsync(entryStream);
                }
            });

        app.MapGet("/download/fromUpload/{linkId:guid}/{fileName}",
            async (Guid linkId, string fileName, UploadLinksService linksService) =>
            {
                var file = await linksService.GetFile(linkId, fileName);
                return Results.File(file, "application/octet-stream");
            });

        app.MapGet("/download/fromRequest/{linkId:guid}/{slotId:guid}/{fileName}", async (Guid slotId, Guid linkId,
            string fileName, AppDbContext dbContext, IStoreService storeService) =>
        {
            var link = await dbContext.RequestLinks.Include(requestLink => requestLink.UploadSlots)
                .FirstOrDefaultAsync(x => x.LinkId == linkId);
            if (link is null)
            {
                app.Logger.LogError("Unable to find link by id {}", linkId);
                return Results.NotFound();
            }

            if (link.UploadSlots.All(slot => slot.RequestLinkUploadSlotId != slotId))
            {
                app.Logger.LogError("Unable to find upload slot with id {} for link {}", slotId, link.LinkTitle);
                return Results.NotFound();
            }

            var file = await storeService.GetFile($"{linkId}/{slotId}/{fileName}");
            return Results.File(file, "application/octed-stream");
        });

        app.MapGet("/download/fromRequest/{linkId:guid}/{slotId:guid}", async (Guid slotId, Guid linkId, AppDbContext dbContext,
            IStoreService storeService, HttpContext context) =>
        {
            var link = await dbContext.RequestLinks.Include(requestLink => requestLink.UploadSlots)
                .FirstOrDefaultAsync(x => x.LinkId == linkId);
            if (link is null)
            {
                app.Logger.LogError("Unable to find link by id {}", linkId);
                context.Response.StatusCode = 404;
            }

            var slot = link!.UploadSlots.FirstOrDefault(slot => slot.RequestLinkUploadSlotId == slotId);
            if (slot == null)
            {
                app.Logger.LogError("Unable to find upload slot with id {} for link {}", slotId, link.LinkTitle);
                context.Response.StatusCode = 404;
            }

            var files = await storeService.ListFolder($"{linkId}/{slotId}");
            var sanitizedTitle = FileUtils.SanitizePath(slot!.RequestLink.LinkTitle!);

            // Set the Content-Disposition header for the file name
            context.Response.Headers.ContentDisposition = $"attachment; filename=\"{sanitizedTitle}.zip\";";
            // Set the content type for ZIP files
            context.Response.Headers.ContentType = "application/zip";

            using var archive = new ZipArchive(context.Response.BodyWriter.AsStream(), ZipArchiveMode.Create, true);
            foreach (var file in files)
            {
                await using var fs = await storeService.GetFile(file.Path);
                var entry = archive.CreateEntry(file.Name, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await fs.CopyToAsync(entryStream);
            }
        });
    }
}