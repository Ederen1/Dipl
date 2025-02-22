using Dipl.Business.Services.Interfaces;

namespace Dipl.Business.Services;

public class UploadLinksService(
    AppDbContext dbContext,
    IStoreService storeService)
{
    public async Task<Stream> GetFile(Guid linkId, string fileName)
    {
        var link = await dbContext.UploadLinks.FindAsync(linkId) ?? throw new Exception("Link not found");
        return await storeService.GetFile($"{link.LinkId}/{fileName}");
    }
}