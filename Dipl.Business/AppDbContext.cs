using Dipl.Business.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dipl.Business;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<UploadLink> UploadLinks { get; set; }
    public DbSet<RequestLink> RequestLinks { get; set; }
    public DbSet<RequestLinkUploadSlot> RequestLinkUploadSlots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Store AllowedExtensions as a comma-separated string in the database.
        modelBuilder.Entity<RequestLink>().Property(e => e.AllowedExtensions).HasConversion(a => string.Join(',', a),
            b => b.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        InitializeData(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private static void InitializeData(ModelBuilder modelBuilder)
    {
        // Seed a default guest user.
        modelBuilder.Entity<User>().HasData(new User
        {
            UserId = User.GuestUserId,
            Email = "guest@example.com",
            UserName = "Guest"
        });
    }
}