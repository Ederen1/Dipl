using System.IO.Compression;
using Dipl.Business.Entities;
using Dipl.Business.Models;
using Dipl.Business.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dipl.Business.Services;

public class UploadLinksService(
    AppDbContext dbContext,
    EmailSenderService emailSenderService,
    UsersService usersService,
    ILogger<UploadLinksService> logger,
    IStoreService storeService)
{
    public async Task<Guid> GenerateUploadLink(CreateUploadLinkModel model)
    {
        var createdBy = await usersService.GetCurrentUser();

        var link = new UploadLink
        {
            CreatedById = createdBy.UserId,
            Message = model.MessageForUser,
            LinkTitle = model.LinkTitle
        };

        await dbContext.UploadLinks.AddAsync(link);
        await dbContext.SaveChangesAsync();

        await emailSenderService.NotifyUserUploaded(link, model);
        return link.LinkId;
    }

    public async Task<Stream> GetFile(Guid linkId, string fileName)
    {
        var link = await dbContext.UploadLinks.FindAsync(linkId) ?? throw new Exception("Link not found");
        return await storeService.GetFile($"{link.LinkId}/{fileName}");
    }
}