using Dipl.Business.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Dipl.Business.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServiceLayer(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<FileManagerService>();
        serviceCollection.AddScoped<LinksService>();
        serviceCollection.AddScoped<InitializationService>();
        serviceCollection.AddScoped<UsersService>();
        serviceCollection.AddScoped<IStoreService, FileStoreService>(_ => new FileStoreService("folder/"));
    }
}