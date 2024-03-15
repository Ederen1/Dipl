using Dipl.Business.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dipl.Business;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Link> Links { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Group> Groups { get; set; }
}
