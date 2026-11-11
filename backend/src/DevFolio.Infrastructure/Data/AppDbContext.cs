using Microsoft.EntityFrameworkCore;
using DevFolio.Core.Entities;

namespace DevFolio.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects { get; set; }
    public DbSet<User> Users { get; set; }

    // Pre-computed BCrypt hash (cost 11) for the seed password "admin123".
    // Generated once via BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 11).
    // Do NOT call HashPassword here: a random salt makes the EF model
    // non-deterministic and forces a new migration on every run.
    private const string AdminPasswordHash = "$2a$11$4fPY7TGdNWkg4WsB4urB3eQUK3zld88tiY316oJnt6P2qJT.QNOI.";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "admin", PasswordHash = AdminPasswordHash }
        );
    }
}