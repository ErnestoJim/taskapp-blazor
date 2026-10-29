using Microsoft.EntityFrameworkCore;
using TaskApp.Models;

namespace TaskApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.HasIndex(t => t.IsDone);
        });
    }
}
