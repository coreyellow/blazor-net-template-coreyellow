using BlazorNetApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorNetApp.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Seed some initial data
        modelBuilder.Entity<TodoItem>().HasData(
            new TodoItem
            {
                Id = 1,
                Title = "Welcome to Net App",
                Description = "This is a sample TODO item to get you started",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new TodoItem
            {
                Id = 2,
                Title = "Explore the API",
                Description = "Check out the Swagger UI at /swagger",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
