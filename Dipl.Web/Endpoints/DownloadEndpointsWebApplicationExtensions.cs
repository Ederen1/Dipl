using System.IO.Compression;
using Dipl.Business;
using Dipl.Business.Services;
using Dipl.Business.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dipl.Web.Endpoints;

public static class DownloadEndpointsWebApplicationExtensions
{
    public static void MapDownloadEndpoints(this WebApplication app)
    {
        app.MapGet("/download/{linkId:guid}", async (Guid linkId, AppDbContext dbContext, IStoreService storeService, HttpContext context) =>
        {
            var link = await dbContext.Links.FindAsync(linkId) ?? throw new Exception("Link not found");
            var files = await storeService.List(link.Folder);

            // TODO: maybe estimate final zip size?
            // context.Response.Headers.ContentLength = files.Sum(x => x.Size);

            using var archive = new ZipArchive(context.Response.BodyWriter.AsStream(), ZipArchiveMode.Create, true);
            foreach (var file in files)
            {
                await using var fs = File.OpenRead(file.Path);
                var entry = archive.CreateEntry(file.Name, CompressionLevel.NoCompression);
                await using var entryStream = entry.Open();
                await fs.CopyToAsync(entryStream);
            }
        });

        app.MapGet("/download/{linkId:guid}/{fileName}", async (Guid linkId, string fileName, LinksService linksService) =>
        {
            var file = await linksService.GetFile(linkId, fileName);
            return Results.File(file, "application/octet-stream");
        });
    } 
}