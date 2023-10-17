using Dipl.Business.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dipl.Business;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Link> Links { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
}