using System.Diagnostics;
using System.IO.Compression;
using Dipl.Business;
using Dipl.Business.Entities;
using Dipl.Business.Services;
using Dipl.Business.Services.Interfaces;
using Dipl.Common.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dipl.Web.Endpoints;

public static class DownloadEndpointsWebApplicationExtensions
{
    public static void MapDownloadEndpoints(this WebApplication app)
    {
        app.MapGet("/download/fromUpload/{linkId:guid}", async (Guid linkId, AppDbContext dbContext,
            IStoreService storeService, HttpContext context, [FromHeader(Name = "X-DownloadAuth")] string? password) =>
        {
            var link = await dbContext.UploadLinks.FindAsync(linkId);
            if (link is null)
                return Results.NotFound("Link not found in database");

            var files = await storeService.ListFolder(link.LinkId.ToString());
            if (files is null)
            {
                return Results.NotFound("Link folder not found or empty");
            }

            if (link.Salt is not null && !LinkSecurityService.PasswordMatchesLink(link, password!))
                return Results.Unauthorized();
            
            var sanitizedTitle = FileUtils.SanitizePath(link.LinkTitle!);
            context.Response.Headers.ContentDisposition = $"attachment; filename=\"{sanitizedTitle}.zip\";";
            context.Response.Headers.ContentType = "application/zip";
            
            using var archive = new ZipArchive(context.Response.BodyWriter.AsStream(), ZipArchiveMode.Create, true);
            try
            {
                foreach (var file in files)
                {
                    await using var baseFile = await storeService.GetFile(file.Path);

                    var entry = archive.CreateEntry(file.Name, CompressionLevel.Optimal);
                    await using var entryStream = entry.Open();
                    if (link.Salt is not null)
                    {
                        await using var cryptoStream =
                            LinkSecurityService.DecryptDataAsync(link, password!, baseFile);
                        await cryptoStream.CopyToAsync(entryStream);
                    }
                    else
                    {
                        await baseFile.CopyToAsync(entryStream);
                    }
                }
            }
            catch (Exception e)
            {
                app.Logger.LogError("Loading file from storage failed, {}", e);
                return Results.NotFound("Loading from storage failed");
            }


            return Results.Empty;
        });

        app.MapGet("/download/fromUpload/{linkId:guid}/{fileName}", async (Guid linkId, string fileName,
            AppDbContext dbContext, HttpContext context, IStoreService storeService, ILogger<Program> logger,
            [FromHeader(Name = "X-DownloadAuth")] string? password) =>
        {
            var link = await dbContext.UploadLinks.FindAsync(linkId);
            if (link is null)
            {
                return Results.NotFound("Link not found in database");
            }
            
            if (link.Salt is not null && !LinkSecurityService.PasswordMatchesLink(link, password!))
                return Results.Unauthorized();

            context.Response.Headers.ContentDisposition = $"attachment; filename=\"{fileName}\";";
            var timestamp = Stopwatch.GetTimestamp();
            try
            {
                await using var baseFile = await storeService.GetFile($"{link.LinkId}/{fileName}");
                if (link.Salt is not null)
                {
                    await using var cryptoStream =
                        LinkSecurityService.DecryptDataAsync(link, password!, baseFile);
                    await using var writer = context.Response.BodyWriter.AsStream();
                    await cryptoStream.CopyToAsync(writer);
                }
                else
                {
                    context.Response.Headers.ContentLength = baseFile.Length;
                    await using var writer = context.Response.BodyWriter.AsStream();
                    await baseFile.CopyToAsync(writer);
                }
            }
            catch (Exception e)
            {
                logger.LogError("Loading file from storage failed, {}", e);
                return Results.NotFound("Loading from storage failed");
            }

            logger.LogWarning("Time elapsed {}", Stopwatch.GetElapsedTime(timestamp));
            
            return Results.Empty;
        });

        app.MapGet("/download/fromRequest/{linkId:guid}/{slotId:guid}/{fileName}", async (Guid slotId, Guid linkId,
            string fileName, AppDbContext dbContext, HttpContext context, IStoreService storeService,
            [FromHeader(Name = "X-DownloadAuth")] string? password) =>
        {
            var link = await dbContext.RequestLinks.Include(requestLink => requestLink.UploadSlots)
                .FirstOrDefaultAsync(x => x.LinkId == linkId);

            if (link is null)
            {
                app.Logger.LogError("Unable to find link by id {}", linkId);
                return Results.NotFound("Unable to find link by id");
            }

            if (link.UploadSlots.All(slot => slot.RequestLinkUploadSlotId != slotId))
            {
                app.Logger.LogError("Unable to find upload slot with id {} for link {}", slotId, link.LinkTitle);
                return Results.NotFound("Upload slot not found for link");
            }

            if (link.Salt is not null && !LinkSecurityService.PasswordMatchesLink(link, password!))
                return Results.Unauthorized();
            
            context.Response.Headers.ContentDisposition = $"attachment; filename=\"{fileName}\";";
            
            try
            {
                await using var baseFile = await storeService.GetFile($"{linkId}/{slotId}/{fileName}");
                
                if (link.Salt is not null)
                {
                    await using var cryptoStream =
                        LinkSecurityService.DecryptDataAsync(link, password!, baseFile);
                    await using var writer = context.Response.BodyWriter.AsStream();
                    await cryptoStream.CopyToAsync(writer);
                }
                else
                {
                    context.Response.Headers.ContentLength = baseFile.Length;
                    await using var writer = context.Response.BodyWriter.AsStream();
                    await baseFile.CopyToAsync(writer);
                }
            }
            catch (Exception e)
            {
                app.Logger.LogError("Loading file from storage failed, {}", e);
                return Results.NotFound("Loading from storage failed");
            }

            return Results.Empty;
        });

        app.MapGet("/download/fromRequest/{linkId:guid}/{slotId:guid}", async (Guid slotId, Guid linkId,
            AppDbContext dbContext, IStoreService storeService, HttpContext context,
            [FromHeader(Name = "X-DownloadAuth")] string? password) =>
        {
            var link = await dbContext.RequestLinks.Include(requestLink => requestLink.UploadSlots)
                .FirstOrDefaultAsync(x => x.LinkId == linkId);

            if (link is null)
            {
                app.Logger.LogError("Unable to find link by id {}", linkId);
                return Results.NotFound("Link not found");
            }

            var slot = link!.UploadSlots.FirstOrDefault(slot => slot.RequestLinkUploadSlotId == slotId);
            if (slot == null)
            {
                app.Logger.LogError("Slot {} for link {} not found", slotId, linkId);
                return Results.NotFound("Slot for link not found");
            }

            var files = await storeService.ListFolder($"{linkId}/{slotId}");
            if (files is null)
            {
                app.Logger.LogError("Link folder '{}/{}' does not exist or is empty", linkId, slotId);
                return Results.NotFound("Link folder does not exist or is empty");
            }
            
            if (link.Salt is not null && !LinkSecurityService.PasswordMatchesLink(link, password!))
                return Results.Unauthorized();

            var sanitizedTitle = FileUtils.SanitizePath(slot!.RequestLink.LinkTitle!);

            // Set the Content-Disposition header for the file name
            context.Response.Headers.ContentDisposition = $"attachment; filename=\"{sanitizedTitle}.zip\";";
            // Set the content type for ZIP files
            context.Response.Headers.ContentType = "application/zip";

            try
            {
                using var archive = new ZipArchive(context.Response.BodyWriter.AsStream(), ZipArchiveMode.Create, true);
                foreach (var file in files)
                {
                    await using var baseFile = await storeService.GetFile(file.Path);

                    var entry = archive.CreateEntry(file.Name, CompressionLevel.Optimal);
                    await using var entryStream = entry.Open();
                    if (link.Salt is not null)
                    {
                        await using var cryptoStream =
                            LinkSecurityService.DecryptDataAsync(link, password!, baseFile);
                        await cryptoStream.CopyToAsync(entryStream);
                    }
                    else
                    {
                        await baseFile.CopyToAsync(entryStream);
                    }
                }
            }
            catch (Exception e)
            {
                app.Logger.LogError("Loading file from storage failed, {}", e);
                return Results.NotFound("Loading from storage failed");
            }


            return Results.Empty;
        });
    }
}