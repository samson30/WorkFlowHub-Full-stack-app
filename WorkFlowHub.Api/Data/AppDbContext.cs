using Microsoft.EntityFrameworkCore;
using WorkFlowHub.Api.Models;

namespace WorkFlowHub.Api.Data;

public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        
    }
    public DbSet<User> Users => Set<User>();

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
 modelBuilder.Entity<Project>()
        .HasMany(p => p.Tasks)
        .WithOne(t => t.Project)
        .HasForeignKey(t => t.ProjectId)
        .OnDelete(DeleteBehavior.Cascade);

    }

}
