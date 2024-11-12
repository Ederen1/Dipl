using Dipl.Web.Services;

namespace Dipl.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddWebServiceLayer(this IServiceCollection services)
    {
        services.AddScoped<FileManagerService>();
    }
}