using Dipl.Business.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dipl.Business.Test;

public class DbTest
{
    private readonly AppDbContext _dbContext;

    public DbTest()
    {
        var collection = new ServiceCollection();
        var connection = new SqliteConnection("Datasource=:memory:");
        connection.Open();

        collection.AddDbContext<AppDbContext>(opts =>
        {
            opts.UseLazyLoadingProxies();
            opts.UseSqlite(connection);
        });

        var res = collection.BuildServiceProvider();
        _dbContext = res.GetService<AppDbContext>()!;
        _dbContext.Database.EnsureCreated();
    }

    [Fact]
    public void Test()
    {
        var user = _dbContext.Users.Add(new User { Email = "Test@example.com", UserName = "Test" });
        var permission = _dbContext.Permissions.Add(new Permission
        {
            Group = new Group { Name = "Test" },
            Read = true,
            Write = true
        });
        user.Entity.UploadLinks.Add(new UploadLink
        {
            UploadLinkId = Guid.NewGuid(),
            CreatedById = user.Entity.UserId,
            Folder = "/some/folder",
            Permission = permission.Entity
        });

        _dbContext.SaveChanges();

        Assert.Single(user.Entity.UploadLinks);
        Assert.NotNull(permission.Entity.Group);
    }
}