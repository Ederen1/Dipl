using Dipl.Business.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dipl.Business;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<UploadLink> UploadLinks { get; set; }
    public DbSet<RequestLink> RequestLinks { get; set; }
    public DbSet<RequestLinkUploadSlot> RequestLinkUploadSlots { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Group> Groups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        InitializeData(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private static void InitializeData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>().HasData(new Group
        {
            GroupId = Group.GuestGrupId,
            Name = "Guest"
        });

        modelBuilder.Entity<Permission>().HasData(new Permission
        {
            PermissionId = Permission.GuestPermissionId,
            GroupId = Group.GuestGrupId,
            Read = true,
            Write = false
        });

        modelBuilder.Entity<User>().HasData(new User
        {
            UserId = User.GuestUserId,
            Email = "guest@example.com",
            UserName = "Guest"
        });

        modelBuilder.Entity("GroupUser").HasData(new
        {
            GroupsGroupId = Group.GuestGrupId,
            UsersUserId = User.GuestUserId
        });
    }
}