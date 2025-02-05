using System.IO.Compression;
using System.Net;
using Dipl.Business;
using Dipl.Business.Services;
using Dipl.Business.Services.Interfaces;

namespace Dipl.Web.Endpoints;

public static class DownloadEndpointsWebApplicationExtensions
{
    public static void MapDownloadEndpoints(this WebApplication app)
    {
        app.MapGet("/download/{linkId:guid}",
            async (Guid linkId, AppDbContext dbContext, IStoreService storeService, HttpContext context) =>
            {
                var link = await dbContext.UploadLinks.FindAsync(linkId) ?? throw new Exception("Link not found");
                var files = await storeService.List(link.Folder);

                // Set the Content-Disposition header for the file name
                context.Response.Headers.ContentDisposition =
                    $"attachment; filename=\"{WebUtility.UrlEncode(link.LinkTitle)}.zip\";";
                // Set the content type for ZIP files
                context.Response.Headers.ContentType = "application/zip";

                using var archive = new ZipArchive(context.Response.BodyWriter.AsStream(), ZipArchiveMode.Create, true);
                foreach (var file in files)
                {
                    await using var fs = await storeService.GetFile(file.Path);
                    var entry = archive.CreateEntry(file.Name, CompressionLevel.NoCompression);
                    await using var entryStream = entry.Open();
                    await fs.CopyToAsync(entryStream);
                }
            });

        app.MapGet("/download/{linkId:guid}/{fileName}",
            async (Guid linkId, string fileName, UploadLinksService linksService) =>
            {
                var file = await linksService.GetFile(linkId, fileName);
                return Results.File(file, "application/octet-stream");
            });
    }
}